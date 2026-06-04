
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace StudentPortal.ServiceDefaults.Extensions;

public static class LoggingExtension
{
     public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
        {
            var env = builder.Environment.EnvironmentName;
            var serviceName = builder.Configuration["OTEL_SERVICE_NAME"]
                ?? builder.Environment.ApplicationName;

            var replicaIndex = builder.Configuration["ASPNETCORE_REPLICA_INDEX"];
            var instanceId = !string.IsNullOrEmpty(replicaIndex)
                ? $"{serviceName}-{replicaIndex}"
                : serviceName;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Is(env == "Development" ? LogEventLevel.Debug : LogEventLevel.Information)
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("InstanceId", instanceId)
                .Enrich.WithProperty("ReplicaIndex", replicaIndex ?? "0")
                .Enrich.WithProperty("Environment", env)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    $"logs/{serviceName}-.json",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            builder.Logging.ClearProviders(); 
            builder.Logging.AddSerilog(Log.Logger); 

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.AddOtlpExporter();
            });

            builder.Logging.SetMinimumLevel(LogLevel.Information);

            return builder;
        }
    }
