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
using StudentPortal.ServiceDefaults.Memory;
using StudentPortal.ServiceDefaults.Redis;
using StudentPortal.ServiceDefaults.Hybrid;
using StudentPortal.CourseCatalogService.GrpcServer.Mapping;
using StudentPortal.CourseCatalogService.GrpcServer.Services;
using StudentPortal.CourseCatalogService.BLL.Consumers.Lessons;
using StudentPortal.CourseCatalogService.BLL.Consumers.Materials;
using StudentPortal.CourseCatalogService.BLL.Consumers.Modules;
using StudentPortal.CourseCatalogService.BLL.Consumers.UserRoles;
using StudentPortal.CourseCatalogService.BLL.Consumers.Roles;
using StudentPortal.CourseCatalogService.BLL.Consumers.Users;
using StudentPortal.CourseCatalogService.BLL.Consumers.StudentCourses;
using MassTransit;
using StudentPortal.Shared.Events.Lessons;
using StudentPortal.Shared.Events.Materials;
using StudentPortal.Shared.Events.Modules;
using StudentPortal.Shared.Events.Roles;
using StudentPortal.Shared.Events.Users;
using StudentPortal.Shared.Events.UserRoles;

using StudentPortal.CourseCatalogService.BLL.Cache;
using StudentPortal.CourseCatalogService.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();
builder.AddOpenTelemetryTracing();
builder.Services.AddCorrelationIdForwarding();
builder.Services.AddGrpcWithObservability(builder.Environment);


builder.Services.AddDbContext<CourseCatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services
    .AddKeycloakAuthentication(builder.Configuration)
    .AddPermissionAuthorization();

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

builder.Services.AddScoped<IEntityCacheInvalidationService<Course>, CourseCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<Lesson>, LessonCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<Material>, MaterialCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<Module>, ModuleCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<Role>, RoleCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<UserRole>, UserRoleCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<User>, UserCacheInvalidationService>();



builder.Services.AddRedis(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RoleDeletedEventConsumer>();
    x.AddConsumer<RoleCreatedEventConsumer>();
    x.AddConsumer<UserCreatedEventConsumer>(); 
    x.AddConsumer<UserUpdatedEventConsumer>(); 
    x.AddConsumer<UserDeletedEventConsumer>();
    x.AddConsumer<LessonCreatedEventConsumer>();
    x.AddConsumer<LessonDeletedEventConsumer>();
    x.AddConsumer<MaterialCreatedEventConsumer>();
    x.AddConsumer<MaterialDeletedEventConsumer>();
    x.AddConsumer<ModuleCreatedEventConsumer>();
    x.AddConsumer<ModuleDeletedEventConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        cfg.ConfigureEndpoints(context);
    });
});





builder.Host.UseSerilog();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);




builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>(); 

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(ISortHelper<>), typeof(SortHelper<>));

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>(); 



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithKeycloak(builder.Configuration, "StudentPortal Catalog API");

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
app.UseSwaggerWithKeycloak();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.MapHealthChecks("/health");

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCorrelationId();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<CourseCatalogGrpcServiceImpl>();
app.MapGrpcServicesWithReflection();


app.Run();