using System.Text.Json;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.AggregatorService.Services;
using StudentPortal.AggregatorService.Clients;
using StudentPortal.ServiceDefaults.Metrics;
using StudentPortal.EnrollmentService.Grpc;
using StudentPortal.AggregatorService.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();
builder.Services.AddGrpcWithObservability(builder.Environment);
builder.Services.AddServiceDiscovery();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis")
                            ?? throw new InvalidOperationException("Redis connection string not found.");
    options.InstanceName = "AggregatorCache:";
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServiceDiscovery();
builder.Services.AddTransient<EnrollmentAggregatorService>();
builder.Services.AddTransient<CourseAggregatorService>();


builder.Services.AddGrpcClientWithDefaults<EnrollmentGrpcService.EnrollmentGrpcServiceClient>(
    "https://enrollmentservice-api", 
    builder.Environment)
    .AddServiceDiscovery()
    .AddGrpcResilienceHandler(ResilienceProfile.WriteOptimized);  

builder.Services.AddGrpcClientWithDefaults<StudentPortal.CourseCatalog.Grpc.CourseCatalog.CourseCatalogClient>(
    "https://coursecatalogservice-api", 
    builder.Environment)
    .AddServiceDiscovery()
    .AddGrpcResilienceHandler(ResilienceProfile.ReadOptimized); 

builder.Services.AddGrpcClientWithDefaults<StudentPortal.Discussion.Grpc.Discussion.DiscussionClient>(
    "https://discussionservice-api", 
    builder.Environment)
    .AddServiceDiscovery()
    .AddGrpcResilienceHandler(ResilienceProfile.Standard);

builder.Services.AddSingleton<CacheMetrics>();


builder.Services.AddScoped<CourseCatalogGrpcClient>();
builder.Services.AddScoped<DiscussionGrpcClient>();
builder.Services.AddScoped<EnrollmentGrpcClient>();


builder.Services.AddTransient<CourseAggregatorService>();
builder.Services.AddTransient<EnrollmentAggregatorService>();

builder.Services.AddCorrelationIdForwarding();

builder.Services.AddSingleton<CacheMetrics>();

builder.Services.AddHealthChecks()
    .AddCheck<CourseCatalogServiceHealthCheck>(
        "coursecatalog-grpc",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "grpc", "downstream", "ready" })
    .AddCheck<DiscussionServiceHealthCheck>(
        "discussion-grpc",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "grpc", "downstream", "ready" })
    .AddCheck<EnrollmentServiceHealthCheck>(
        "enrollment-grpc",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "grpc", "downstream", "ready", "critical" });


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCorrelationId();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();