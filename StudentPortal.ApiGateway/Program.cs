using StudentPortal.ApiGateway.Middleware;
using StudentPortal.ServiceDefaults.Extensions;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddServiceDiscovery();

builder.Services
    .AddKeycloakAuthentication(builder.Configuration)
    .AddPermissionAuthorization();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async transformContext =>
        {
            var correlationId = transformContext.HttpContext.Items["X-Correlation-Id"]?.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(
                    "X-Correlation-Id",
                    correlationId);
            }
            await ValueTask.CompletedTask;
        });
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCorrelationId();            
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<GatewayLoggingMiddleware>();
app.UseMiddleware<GatewayRequestMiddleware>();
app.UseMiddleware<TimeoutMiddleware>();    
app.UseMiddleware<YarpProxyLoggingMiddleware>();
app.MapReverseProxy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
await app.RunAsync();