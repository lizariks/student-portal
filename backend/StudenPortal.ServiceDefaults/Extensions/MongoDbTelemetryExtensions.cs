namespace StudentPortal.ServiceDefaults.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Diagnostics;

    public static class MongoDbTelemetryExtensions
    {
        public static IServiceCollection AddMongoDbTelemetry(this IServiceCollection services, string connectionString)
        {
            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);

            mongoSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    var activity = Activity.Current;
                    if (activity != null)
                    {
                        activity.SetTag("db.system", "mongodb");
                        activity.SetTag("db.operation", e.CommandName);
                        activity.SetTag("db.statement", e.Command.ToString());
                        activity.SetTag("db.database", e.DatabaseNamespace.DatabaseName);
                    }
                });
            };

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings));

            return services;
        }
    }