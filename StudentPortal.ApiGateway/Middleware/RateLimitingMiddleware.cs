namespace StudentPortal.ApiGateway.Middleware;

using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Yarp.ReverseProxy.Configuration;


    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _config = config;
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

            int requestsPerMinute = GetIntMetadata(routeConfig, "RateLimit:RequestsPerMinute")
                ?? _config.GetValue("Gateway:DefaultRateLimit:RequestsPerMinute", 100);

            int burst = GetIntMetadata(routeConfig, "RateLimit:Burst")
                ?? _config.GetValue("Gateway:DefaultRateLimit:Burst", 20);

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "shared";
            var routeId = routeConfig.RouteId ?? "unknown-route";
            var correlationId = context.Items["X-Correlation-Id"]?.ToString() ?? "unknown";
            var cacheKey = $"rate:{routeId}:{clientIp}";

            var now = DateTime.UtcNow;
            if (!_cache.TryGetValue(cacheKey, out RateLimitCounter? counter))
            {
                counter = new RateLimitCounter { Count = 0, Timestamp = now };
            }

            if (now - counter!.Timestamp > TimeSpan.FromMinutes(1))
            {
                counter.Count = 0;
                counter.Timestamp = now;
            }

            counter.Count++;
            _cache.Set(cacheKey, counter, TimeSpan.FromMinutes(2));

            if (counter.Count > requestsPerMinute + burst)
            {
                Log.Warning(
                    "Rate limit exceeded | {ClientIp} | {RouteId} | {CorrelationId} | {Count}/{Limit}",
                    clientIp, routeId, correlationId, counter.Count, requestsPerMinute);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                context.Response.Headers["X-RateLimit-Limit"] = requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            context.Response.Headers["X-RateLimit-Limit"] = requestsPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] =
                Math.Max(0, requestsPerMinute + burst - counter.Count).ToString();

            await _next(context);
        }

        private static int? GetIntMetadata(RouteConfig routeConfig, string key)
        {
            if (routeConfig.Metadata != null &&
                routeConfig.Metadata.TryGetValue(key, out var val) &&
                int.TryParse(val, out var result))
            {
                return result;
            }
            return null;
        }

        private sealed class RateLimitCounter
        {
            public int Count;
            public DateTime Timestamp;
        }
    }