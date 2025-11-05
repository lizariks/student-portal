using Serilog;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;


namespace StudentPortal.ApiGateway.Middleware;

public class YarpProxyLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeader = "X-Correlation-Id";

        public YarpProxyLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Items[CorrelationHeader]?.ToString() ?? "unknown";

            var endpoint = context.GetEndpoint();
            var routeConfig = endpoint?.Metadata.GetMetadata<RouteConfig>();

            if (routeConfig != null)
            {
                Log.Information(
                    "YARP routing: Route={RouteId} → Cluster={ClusterId} | {Method} {Path} | CorrelationId={CorrelationId}",
                    routeConfig.RouteId,
                    routeConfig.ClusterId,
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }
            else
            {
                Log.Warning(
                    "YARP routing: No route matched | {Method} {Path} | CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }

            await _next(context);

            var errorFeature = context.Features.Get<IForwarderErrorFeature>();

            if (errorFeature != null)
            {
                Log.Error(
                    errorFeature.Exception,
                    "YARP proxy FAILED: Error={Error} | Route={RouteId} → Cluster={ClusterId} | {Method} {Path} | CorrelationId={CorrelationId}",
                    errorFeature.Error,
                    routeConfig?.RouteId ?? "unknown",
                    routeConfig?.ClusterId ?? "unknown",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }
            else if (routeConfig != null)
            {
                Log.Debug(
                    "YARP proxy SUCCESS: Route={RouteId} → Cluster={ClusterId} | Status={StatusCode} | CorrelationId={CorrelationId}",
                    routeConfig.RouteId,
                    routeConfig.ClusterId,
                    context.Response.StatusCode,
                    correlationId);
            }
        }
    }