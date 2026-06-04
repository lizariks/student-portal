namespace StudentPortal.ServiceDefaults.Health;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;


    public static class RedisHealthChecksExtensions
    {
        public static IHealthChecksBuilder AddRedisHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration,
            string connectionName,
            string serviceName,
            int timeoutSeconds = 3)
        {
            var connectionString = configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

            return builder.AddRedis(
                redisConnectionString: connectionString,
                name: $"{serviceName}-redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "redis", "ready" },
                timeout: TimeSpan.FromSeconds(timeoutSeconds));
        }
    }
