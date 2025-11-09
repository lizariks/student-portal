using System.Text.Json;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.AggregatorService.Services;
using StudentPortal.AggregatorService.Clients;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();

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
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddCorrelationIdHttpClient<EnrollmentClient>(client =>
    {
        client.BaseAddress = new Uri("http://enrollmentservice-api");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(5); 
    })
    .AddServiceDiscovery();

builder.Services.AddCorrelationIdHttpClient<CourseCatalogClient>(client =>
    {
        client.BaseAddress = new Uri("http://coursecatalogservice-api");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(5); 
    })
    .AddServiceDiscovery();

builder.Services.AddCorrelationIdHttpClient<DiscussionClient>(client =>
    {
        client.BaseAddress = new Uri("http://discussionservice-api");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddServiceDiscovery();

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