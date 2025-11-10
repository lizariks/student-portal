namespace StudentPortal.ServiceDefaults.Health;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

    public static class MongoHealthChecksExtensions
    {
        public static IHealthChecksBuilder AddMongoHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration,
            string connectionName,
            string serviceName,
            string? databaseName = null,
            int timeoutSeconds = 5)
        {
            var connectionString = configuration.GetConnectionString(connectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

            builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

            return builder.AddMongoDb(
                databaseNameFactory: sp => databaseName ?? "admin",
                name: $"{serviceName}-mongodb",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "mongodb", "ready", "live" },
                timeout: TimeSpan.FromSeconds(timeoutSeconds));
        }
    }
