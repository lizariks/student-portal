using StudentPortal.ApiGateway.Middleware;
using StudentPortal.ServiceDefaults.Extensions;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.AddServiceDefaults();
builder.Services.AddServiceDiscovery();
builder.AddOpenTelemetryTracing();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<CorrelationIdGeneratorMiddleware>();
app.UseMiddleware<GatewayRequestMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseMiddleware<TimeoutMiddleware>();
app.UseMiddleware<GatewayLoggingMiddleware>();

app.MapReverseProxy();
