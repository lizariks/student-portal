using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;

public static class TelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetryTracing(this IHostApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["OTEL_SERVICE_NAME"]
            ?? builder.Environment.ApplicationName;
        var env = builder.Environment.EnvironmentName;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                .AddTelemetrySdk()
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = env
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity?.SetTag("db.commandTimeout", command.CommandTimeout);
                        };
                    })
                    .AddSource("MongoDb.Driver")
                    .AddOtlpExporter();

                if (env == "Development")
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }
                else
                {
                    tracing.SetSampler(new TraceIdRatioBasedSampler(0.2));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter();
            });

        return builder;
    }
}