namespace StudentPortal.CourseCatalogService.Apii.MiddleWare;
using Microsoft.Extensions.Logging;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        var userId = context.User?.FindFirst("sub")?.Value 
                    ?? context.User?.FindFirst("userId")?.Value 
                    ?? "anonymous";

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("UserId", userId))
        using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path))
        using (Serilog.Context.LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            _logger.LogInformation("HTTP {RequestMethod} {RequestPath} started", 
                context.Request.Method, 
                context.Request.Path);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                sw.Stop();

                var level = context.Response.StatusCode >= 500 
                    ? LogLevel.Error 
                    : context.Response.StatusCode >= 400 
                        ? LogLevel.Warning 
                        : LogLevel.Information;

                _logger.Log(level, 
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                
                _logger.LogError(ex,
                    "HTTP {RequestMethod} {RequestPath} failed after {Elapsed:0.0000} ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.Elapsed.TotalMilliseconds);
                
                throw;
            }
        }
    }
}