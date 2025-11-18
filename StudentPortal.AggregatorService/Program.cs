using System.Text.Json;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.AggregatorService.Services;
using StudentPortal.AggregatorService.Clients;
using StudentPortal.ServiceDefaults.Metrics;
using StudentPortal.EnrollmentService.Grpc;
using StudentPortal.CourseCatalog.Grpc;
using StudentPortal.Discussion.Grpc;


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


builder.Services.AddGrpcClient<EnrollmentGrpcService.EnrollmentGrpcServiceClient>(options =>
{
    options.Address = new Uri("https://enrollmentservice-api");
})
.ConfigureChannel(channelOptions =>
{
    channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
    channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
})
.AddServiceDiscovery()
.AddGrpcResilienceHandler(ResilienceProfile.Standard);

builder.Services.AddGrpcClient<StudentPortal.CourseCatalog.Grpc.CourseCatalog.CourseCatalogClient>(options =>
{
    options.Address = new Uri("https://coursecatalogservice-api");
})
.ConfigureChannel(channelOptions =>
{
    channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
    channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
})
.AddServiceDiscovery()
.AddGrpcResilienceHandler(ResilienceProfile.Standard);

builder.Services.AddGrpcClient<StudentPortal.Discussion.Grpc.Discussion.DiscussionClient>(options =>
{
    options.Address = new Uri("https://discussionservice-api");
})
.ConfigureChannel(channelOptions =>
{
    channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
    channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
})
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

await app.RunAsync();