namespace StudentPortal.ServiceDefaults.Background;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StudentPortal.ServiceDefaults.Background.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

public class RabbitMqConsumerService<TEvent, TConsumer> : BackgroundService
    where TEvent : class
    where TConsumer : class, IConsumer<TEvent>
{
    private const int MAX_RETRIES = 3;
    
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumerService<TEvent, TConsumer>> _logger;
    private IChannel? _channel;
    private readonly string _queueName;
    private static readonly ActivitySource ActivitySource = new("StudentPortal.Consumer");

    public RabbitMqConsumerService(
        IConnection connection,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqConsumerService<TEvent, TConsumer>> logger)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = $"{typeof(TConsumer).Name.ToLower()}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dlxName = $"{_queueName}.dlx";
        var dlqName = $"{_queueName}.dlq";
        
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            
            await _channel.ExchangeDeclareAsync(
                exchange: dlxName, 
                type: "topic", 
                durable: true, 
                cancellationToken: stoppingToken);
                
            await _channel.QueueDeclareAsync(
                queue: dlqName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null, 
                cancellationToken: stoppingToken);
            
            await _channel.QueueBindAsync(
                queue: dlqName, 
                exchange: dlxName, 
                routingKey: "#", 
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object> { { "x-dead-letter-exchange", dlxName } },
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var bodyPayload = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(bodyPayload);

                var headers = ea.BasicProperties.Headers;
                
                string? traceparent = headers?.ContainsKey("traceparent") == true 
                    ? Encoding.UTF8.GetString((byte[])headers["traceparent"]) 
                    : null;
                
                string eventType = headers?.ContainsKey("event.type") == true 
                    ? Encoding.UTF8.GetString((byte[])headers["event.type"]) 
                    : typeof(TEvent).Name;
                string correlationId = headers?.ContainsKey("correlation.id") == true 
                    ? Encoding.UTF8.GetString((byte[])headers["correlation.id"]) 
                    : "N/A";
                    
                int retryCount = 0;
                if (headers?.ContainsKey("x-retry-count") == true && headers["x-retry-count"] is byte[] countBytes)
                {
                    int.TryParse(Encoding.UTF8.GetString(countBytes), out retryCount);
                }

                ActivityContext parentContext = default;
                if (!string.IsNullOrEmpty(traceparent))
                {
                    ActivityContext.TryParse(traceparent, null, out parentContext);
                }

                var tags = new List<KeyValuePair<string, object?>>
                {
                    new("messaging.system", "rabbitmq"),
                    new("messaging.destination", _queueName),
                    new("message.correlation_id", correlationId),
                    new("event.type", eventType),
                    new("attempt_number", retryCount + 1)
                };

                using (var activity = ActivitySource.StartActivity(
                    $"{_queueName} consume",
                    ActivityKind.Consumer,
                    parentContext,
                    tags,
                    null,
                    default))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var loggerScope = scope.ServiceProvider.GetRequiredService<ILogger<TConsumer>>();
                        var eventTracker = scope.ServiceProvider.GetService<IEventTracker>(); 
                        
                        using (loggerScope.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
                        {
                            try
                            {
                                var eventMessage = JsonSerializer.Deserialize<TEvent>(message, new JsonSerializerOptions 
                                { 
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                                });

                                if (eventMessage != null)
                                {
                                    var eventIdProperty = typeof(TEvent).GetProperty("EventId");
                                    if (eventIdProperty != null && eventTracker != null) 
                                    {
                                        var eventIdValue = eventIdProperty.GetValue(eventMessage);
                                        if (eventIdValue is Guid eventId)
                                        {
                                            if (await eventTracker.IsEventProcessedAsync(eventId))
                                            {
                                                loggerScope.LogWarning("Duplicate event received and skipped: EventId={EventId}", eventId);
                                                activity?.SetStatus(ActivityStatusCode.Ok, "Skipped duplicate");
                                                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                                                return; 
                                            }
                                            
                                            await eventTracker.MarkEventAsProcessedAsync(eventId); 
                                        }
                                    }
                                    
                                    loggerScope.LogInformation(
                                        "Processing event {EventType} (Attempt {Attempt}) with CorrelationId={CorrelationId}",
                                        eventType, retryCount + 1, correlationId);

                                    var consumerLogic = scope.ServiceProvider.GetRequiredService<TConsumer>();
                                    await consumerLogic.Consume(eventMessage, stoppingToken);
                                }

                                activity?.SetStatus(ActivityStatusCode.Ok);
                                
                                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                            }
                            catch (JsonException ex)
                            {
                                loggerScope.LogError(ex, "Deserialization failed for event {EventType}. Permanent failure, routing to DLQ.", eventType);
                                activity?.SetStatus(ActivityStatusCode.Error, "Deserialization failure");
                                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                activity?.SetStatus(ActivityStatusCode.Error, "Transient business logic failure");

                                if (retryCount >= MAX_RETRIES)
                                {
                                    loggerScope.LogError(ex, "Business logic failed after {Max} attempts. Exhausted retries, routing to DLQ.", MAX_RETRIES);
                                    await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
                                }
                                else
                                {
                                    loggerScope.LogWarning(ex, "Business logic failed (Attempt {Attempt}/{Max}). Requeuing with incremented counter.", retryCount + 1, MAX_RETRIES);
                                    
                                    var newProperties = new BasicProperties
                                    {
                                        Headers = new Dictionary<string, object?>(headers ?? new Dictionary<string, object>())
                                        {
                                            ["x-retry-count"] = Encoding.UTF8.GetBytes((retryCount + 1).ToString())
                                        },
                                        DeliveryMode = ea.BasicProperties.DeliveryMode,
                                        ContentType = ea.BasicProperties.ContentType,
                                        ContentEncoding = ea.BasicProperties.ContentEncoding,
                                        CorrelationId = ea.BasicProperties.CorrelationId,
                                        MessageId = ea.BasicProperties.MessageId
                                    };
                                    
                                    await _channel.BasicPublishAsync(
                                        exchange: "", 
                                        routingKey: _queueName, 
                                        mandatory: true,
                                        basicProperties: newProperties,
                                        body: bodyPayload,
                                        cancellationToken: stoppingToken);
                                    
                                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                                }
                            }
                        }
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Listening for {EventType} on queue {QueueName}.", typeof(TEvent).Name, _queueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer for queue {QueueName} is stopping.", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect or start RabbitMQ consumer for queue {QueueName}", _queueName);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down consumer for queue {QueueName}.", _queueName);
        
        if (_channel != null && _channel.IsOpen)
        {
            try
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing RabbitMQ channel.");
            }
        }

        await base.StopAsync(cancellationToken);
    }
}