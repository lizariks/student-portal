namespace StudentPortal.ServiceDefaults.Background;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StudentPortal.ServiceDefaults.Background.Interfaces;

public class RabbitMqConsumerService<TEvent, TConsumer> : BackgroundService
    where TEvent : class
    where TConsumer : class, IConsumer<TEvent>
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumerService<TEvent, TConsumer>> _logger;
    private IChannel? _channel;
    private readonly string _queueName;

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
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 10,
                global: false,
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var bodyPayload = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(bodyPayload);

                var headers = ea.BasicProperties.Headers;
                string eventType = headers?.ContainsKey("event.type") == true 
                    ? Encoding.UTF8.GetString((byte[])headers["event.type"]) 
                    : typeof(TEvent).Name;
                string correlationId = headers?.ContainsKey("correlation.id") == true 
                    ? Encoding.UTF8.GetString((byte[])headers["correlation.id"]) 
                    : "N/A";

                using (var scope = _serviceProvider.CreateScope())
                {
                    var loggerScope = scope.ServiceProvider.GetRequiredService<ILogger<TConsumer>>();

                    try
                    {
                        var eventMessage = JsonSerializer.Deserialize<TEvent>(message, new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        });

                        if (eventMessage != null)
                        {
                            loggerScope.LogInformation(
                                "Processing event {EventType} with CorrelationId={CorrelationId} from queue {QueueName}",
                                eventType, correlationId, _queueName);

                            var consumerLogic = scope.ServiceProvider.GetRequiredService<TConsumer>();
                            await consumerLogic.Consume(eventMessage, stoppingToken);
                        }

                        await _channel.BasicAckAsync(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            cancellationToken: stoppingToken);
                    }
                    catch (JsonException ex)
                    {
                        loggerScope.LogError(ex, "Deserialization failed for event {EventType}. Routing to Dead Letter.", eventType);
                        await _channel.BasicRejectAsync(
                            deliveryTag: ea.DeliveryTag,
                            requeue: false,
                            cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        loggerScope.LogError(ex, "Business logic failed for event {EventType}. Nacking message.", eventType);
                        await _channel.BasicRejectAsync(
                            deliveryTag: ea.DeliveryTag,
                            requeue: true,
                            cancellationToken: stoppingToken);
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