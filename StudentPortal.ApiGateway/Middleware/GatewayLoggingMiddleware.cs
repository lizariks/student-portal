namespace StudentPortal.ApiGateway.Middleware;

using Serilog;
using System.Diagnostics;

    public class GatewayLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeader = "X-Correlation-Id";

        public GatewayLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            var correlationId = context.Items[CorrelationHeader]?.ToString() ?? "unknown";
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (clientIp == "::1") clientIp = "127.0.0.1";

            Log.Information(
                "[{Time}] Gateway request started {Method} {Path} {CorrelationId} {ClientIp}",
                DateTime.UtcNow,
                context.Request.Method,
                context.Request.Path,
                correlationId,
                clientIp);

            try
            {
                await _next(context);
                sw.Stop();

                var responseSize = context.Response.ContentLength ?? 0;

                if (sw.ElapsedMilliseconds > 3000)
                {
                    Log.Warning(
                        "Slow request {Method} {Path} {ElapsedMs}ms {CorrelationId}",
                        context.Request.Method,
                        context.Request.Path,
                        sw.ElapsedMilliseconds,
                        correlationId);
                }

                Log.Information(
                    "Gateway response {Method} {Path} {StatusCode} {ElapsedMs}ms {ResponseSize}B {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    responseSize,
                    correlationId);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log.Error(
                    ex,
                    "Gateway error {Method} {Path} {ElapsedMs}ms {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    correlationId);
                throw;
            }
        }
    }