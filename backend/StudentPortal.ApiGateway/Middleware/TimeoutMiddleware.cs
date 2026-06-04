namespace StudentPortal.ApiGateway.Middleware;

using Serilog;
using System.Diagnostics;
using Yarp.ReverseProxy.Configuration;


    public class TimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TimeoutMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var routeConfig = endpoint?.Metadata.GetMetadata<RouteConfig>();
            if (routeConfig == null)
            {
                await _next(context);
                return;
            }

            var clusterId = routeConfig.ClusterId ?? "unknown";
            var timeoutKey = $"ReverseProxy:Clusters:{clusterId}:HttpRequest:ActivityTimeout";

            var timeoutValue = _configuration[timeoutKey];
            if (string.IsNullOrEmpty(timeoutValue))
            {
                await _next(context);
                return;
            }

            if (!TimeSpan.TryParse(timeoutValue, System.Globalization.CultureInfo.InvariantCulture, out var timeout))
            {
                Log.Warning("Invalid timeout value '{TimeoutValue}' for cluster {ClusterId}", timeoutValue, clusterId);
                await _next(context);
                return;
            }

            var correlationId = context.Items["X-Correlation-Id"]?.ToString() ?? "unknown";
            var sw = Stopwatch.StartNew();

            using var cts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, cts.Token);

            context.RequestAborted = linkedCts.Token;

            try
            {
                await _next(context);
                sw.Stop();

                Log.Debug(
                    "Request completed for cluster={ClusterId} | {Method} {Path} | {ElapsedMs}ms | CorrelationId={CorrelationId}",
                    clusterId,
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    correlationId);
            }
            catch (OperationCanceledException ex)
            {
                sw.Stop();
                if (cts.IsCancellationRequested)
                {
                    Log.Warning(
                        ex,
                        "Request timed out after {TimeoutSeconds}s for cluster={ClusterId} | {Method} {Path} | CorrelationId={CorrelationId}",
                        timeout.TotalSeconds,
                        clusterId,
                        context.Request.Method,
                        context.Request.Path,
                        correlationId);

                    context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                    await context.Response.WriteAsync("Gateway Timeout");
                }
                else
                {
                    Log.Error(
                        ex,
                        "Request aborted externally for cluster={ClusterId} | {Method} {Path} | CorrelationId={CorrelationId}",
                        clusterId,
                        context.Request.Method,
                        context.Request.Path,
                        correlationId);
                }
            }
        }
    }
