namespace StudentPortal.ApiGateway.Middleware;

using Serilog;

    public class GatewayRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _maxRequestSize;
        private const string ServiceNameKey = "ServiceName";

        public GatewayRequestMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _maxRequestSize = config.GetValue<long>("Gateway:MaxRequestSize", 10 * 1024 * 1024);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > _maxRequestSize)
            {
                Log.Warning(
                    "Request payload too large: {Size} bytes. Limit is {Limit} bytes. Path: {Path}",
                    context.Request.ContentLength.Value,
                    _maxRequestSize,
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync("Payload too large");
                return;
            }

            var serviceName = context.GetEndpoint()?.Metadata
                                  .GetMetadata<RouteNameMetadata>()?.RouteName
                              ?? "unknown-service";

            context.Items[ServiceNameKey] = serviceName;

            Log.Debug(
                "Gateway request metadata added: ServiceName={ServiceName} | Path={Path} | Method={Method}",
                serviceName,
                context.Request.Path,
                context.Request.Method);

            await _next(context);
        }
    }
