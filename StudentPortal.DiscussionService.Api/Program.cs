
using FluentValidation;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Infrastructure.Repositories;
using StudentPortal.DiscussionService.Application.Services;
using StudentPortal.DiscussionService.Application.Behaviors;
using StudentPortal.DiscussionService.Infrastructure;
using StudentPortal.DiscussionService.Infrastructure.Indexes;
using StudentPortal.DiscussionService.Infrastructure.Seeding;
using StudentPortal.DiscussionService.Api.Middleware;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.ServiceDefaults.Health;
using StudentPortal.DiscussionService.GrpcServer.Mapping;
using StudentPortal.DiscussionService.GrpcServer.Service;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddGrpcWithObservability(builder.Environment);

var aspireConn = builder.Configuration.GetConnectionString("studentportal-discussion-service-db")
                 ?? builder.Configuration.GetConnectionString("mongodb");

if (!string.IsNullOrEmpty(aspireConn))
{
    builder.Services.Configure<MongoDbSettings>(options =>
    {
        options.ConnectionString = aspireConn;
        options.DatabaseName = "studentportal-discussion-service-db";
        options.MaxConnectionPoolSize = 100;
        options.MinConnectionPoolSize = 5;
        options.ConnectTimeoutSeconds = 10;
        options.SocketTimeoutSeconds = 10;
    });
}
else
{
    builder.Services.Configure<MongoDbSettings>(
        builder.Configuration.GetSection("MongoDbSettings"));
}

var mongoConnString = !string.IsNullOrEmpty(aspireConn)
    ? aspireConn
    : builder.Configuration.GetSection("MongoDbSettings").GetValue<string>("ConnectionString");

builder.Services.AddMongoDbTelemetry(mongoConnString!);



builder.Services.PostConfigure<MongoDbSettings>(options =>
{
    if (string.IsNullOrEmpty(options.ConnectionString))
        throw new InvalidOperationException("MongoDB ConnectionString is required");
    if (string.IsNullOrEmpty(options.DatabaseName))
        throw new InvalidOperationException("MongoDB DatabaseName is required");
});

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IIndexCreation, MongoIndexCreation>();
builder.Services.AddSingleton<IDataSeeder, DatabaseSeeder>();

builder.Services
    .AddKeycloakAuthentication(builder.Configuration)
    .AddPermissionAuthorization();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
builder.Services.AddScoped<IDiscussionThreadRepository, DiscussionThreadRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();
builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();


builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); 
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
});


builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithKeycloak(builder.Configuration, "StudentPortal Reviews API");
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks()
    .AddMongoHealthCheck(
        builder.Configuration,
        connectionName: "studentportal-discussion-service-db",
        serviceName: "discussionservice",
        databaseName: "studentportal-discussion-service-db",
        timeoutSeconds: 5);

var app = builder.Build();

app.UseSwaggerWithKeycloak();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var indexService = scope.ServiceProvider.GetRequiredService<IIndexCreation>();
    await indexService.CreateIndexesAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}


app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCorrelationId();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health"); 
app.MapControllers();
app.MapGrpcService<DiscussionGrpcServiceImpl>();
app.MapGrpcServicesWithReflection();

await app.RunAsync();
