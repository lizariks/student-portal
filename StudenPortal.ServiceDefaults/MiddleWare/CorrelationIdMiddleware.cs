using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace StudentPortal.ServiceDefaults.MiddleWare;

    public class CorrelationIdMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var existingId)
                ? existingId.ToString()
                : Guid.NewGuid().ToString();

            context.Items[CorrelationHeader] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeader] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                Log.Information("Request started {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await _next(context);

                Log.Information("Request finished {Method} {Path} {StatusCode}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
            }
        }
    }