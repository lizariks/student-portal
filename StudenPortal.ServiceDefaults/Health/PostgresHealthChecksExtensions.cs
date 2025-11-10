namespace StudentPortal.ServiceDefaults.Health;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;


    public static class PostgresHealthChecksExtensions
    {
        public static IHealthChecksBuilder AddPostgresHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration,
            string connectionName,
            string serviceName,
            int timeoutSeconds = 5)
        {
            var connectionString = configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

            return builder.AddNpgSql(
                connectionString: connectionString,
                name: $"{serviceName}-postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "postgresql", "ready", "live" },
                timeout: TimeSpan.FromSeconds(timeoutSeconds));
        }
    }

