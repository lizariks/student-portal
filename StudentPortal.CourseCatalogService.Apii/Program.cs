using Microsoft.EntityFrameworkCore;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.CourseCatalogService.DAL.UoW;
using StudentPortal.CourseCatalogService.DAL.Repositories;
using StudentPortal.CourseCatalogService.DAL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Mapping;
using StudentPortal.CourseCatalogService.DAL.Helpers;
using StudentPortal.CourseCatalogService.BLL.Interfaces;
using StudentPortal.CourseCatalogService.BLL.Services;
using StudentPortal.CourseCatalogService.Apii.MiddleWare;
using StudentPortal.ServiceDefaults.Extensions;
using StudentPortal.ServiceDefaults.Health;
using Serilog;
using AutoMapper;
using StudentPortal.ServiceDefaults.Memory;
using StudentPortal.ServiceDefaults.Redis;
using StudentPortal.ServiceDefaults.Hybrid;
using StudentPortal.CourseCatalogService.BLL.Mapping;
using StudentPortal.CourseCatalogService.GrpcServer.Mapping;
using StudentPortal.CourseCatalogService.GrpcServer.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;


var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();

builder.Services.AddGrpcWithObservability(builder.Environment);

builder.Services.AddAutoMapperWithLogging(
    typeof(CourseProfile).Assembly,
    typeof(CourseCatalogGrpcProfile).Assembly
);

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; 
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});


builder.Services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddSingleton<IHybridCacheService, HybridCacheService>();


builder.Services.AddRedis(builder.Configuration);

builder.Host.UseSerilog();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<CourseCatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString")));



builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(ISortHelper<>), typeof(SortHelper<>));

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHealthChecks()
    .AddCheck<CacheHealthCheck>(
        name: "coursecatalogservice-cache",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "cache", "ready" })
    .AddPostgresHealthCheck(
        configuration: builder.Configuration,
        connectionName: "studentportal-catalogcourses-db",
        serviceName: "coursecatalogservice",
        timeoutSeconds: 5);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseCatalogDbContext>();
    await db.Database.MigrateAsync();
    await CourseCatalogSeedDb.Seed(db);;

}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapHealthChecks("/health");

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<CourseCatalogGrpcServiceImpl>();
app.MapGrpcServicesWithReflection();


app.Run();