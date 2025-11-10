
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

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; 
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

var mongoConnString = builder.Configuration.GetSection("MongoDbSettings")
    .GetValue<string>("ConnectionString");

builder.Services.AddMongoDbTelemetry(mongoConnString!);

var aspireConn = builder.Configuration.GetConnectionString("mongodb");
if (!string.IsNullOrEmpty(aspireConn))
{
    builder.Services.PostConfigure<MongoDbSettings>(options =>
    {
        options.ConnectionString = aspireConn;
    });
}

builder.Services.PostConfigure<MongoDbSettings>(options =>
{
    if (string.IsNullOrEmpty(options.ConnectionString))
        throw new InvalidOperationException("MongoDB ConnectionString is required");
    if (string.IsNullOrEmpty(options.DatabaseName))
        throw new InvalidOperationException("MongoDB DatabaseName is required");
});

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IIndexCreation, MongoIndexCreation>();

builder.Services.AddAutoMapperWithLogging(
    typeof(DiscussionGrpcProfile).Assembly
);

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
builder.Services.AddScoped<IDiscussionThreadRepository, DiscussionThreadRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();
builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();

builder.Services.AddSingleton<IDataSeeder, DatabaseSeeder>();

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
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks()
    .AddMongoHealthCheck(
        builder.Configuration,
        connectionName: "studentportal-discussionervice-db",
        serviceName: "diacussionservice",
        databaseName: "studentportal-discussionervice-db",
        timeoutSeconds: 5);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var indexService = scope.ServiceProvider.GetRequiredService<IIndexCreation>();
    await indexService.CreateIndexesAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}


app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health"); 


app.MapGrpcService<DiscussionGrpcServiceImpl>();
app.MapGrpcServicesWithReflection();

await app.RunAsync();
