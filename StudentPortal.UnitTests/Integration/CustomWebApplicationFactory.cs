using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Data.Sqlite;
using StudentPortal.CourseCatalogService.DAL.Data;
using StudentPortal.ServiceDefaults.Hybrid;
using StudentPortal.ServiceDefaults.Redis;
using Moq;
using MassTransit;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace StudentPortal.UnitTests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            conf.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:redis"] = "localhost:6379,abortConnect=false",
                ["ConnectionStrings:DefaultConnectionString"] = "DataSource=:memory:",
                ["ConnectionStrings:studentportal-catalogcourses-db"] = "DataSource=:memory:",
                ["OTEL_SERVICE_NAME"] = "IntegrationTests"
            });
        });

        builder.ConfigureServices(services =>
        {
            _keepAliveConnection.Open();

            // Remove ALL EF-related registrations to avoid dual-provider conflict
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<CourseCatalogDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                .ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            services.AddDbContext<CourseCatalogDbContext>(options =>
            {
                options.UseSqlite(_keepAliveConnection);
            });
            // Disable authentication and authorization for integration tests
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    TestAuthHandler>("Test", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("Permission:catalog:write", policy =>
                    policy.Requirements.Add(
                        new StudentPortal.ServiceDefaults.Extensions.PermissionRequirement("catalog:write")))
                .AddPolicy("Permission:catalog:delete", policy =>
                    policy.Requirements.Add(
                        new StudentPortal.ServiceDefaults.Extensions.PermissionRequirement("catalog:delete")))
                .AddPolicy("Permission:catalog:manage", policy =>
                    policy.Requirements.Add(
                        new StudentPortal.ServiceDefaults.Extensions.PermissionRequirement("catalog:manage")))
                .AddPolicy("Permission:catalog:read", policy =>
                    policy.Requirements.Add(
                        new StudentPortal.ServiceDefaults.Extensions.PermissionRequirement("catalog:read")));
            
            // Replace real Redis multiplexer with mock
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null) services.Remove(redisDescriptor);
            services.AddSingleton<IConnectionMultiplexer>(Mock.Of<IConnectionMultiplexer>());

            // Replace Redis distributed cache with in-memory
            var redisCacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDistributedCache));
            if (redisCacheDescriptor != null) services.Remove(redisCacheDescriptor);
            services.AddDistributedMemoryCache();

            // Replace IRedisCacheService with mock
            var redisSvcDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRedisCacheService));
            if (redisSvcDescriptor != null) services.Remove(redisSvcDescriptor);
            services.AddSingleton(Mock.Of<IRedisCacheService>());

            // Replace IHybridCacheService with a passthrough fake that always invokes the factory
            var hybridSvcDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHybridCacheService));
            if (hybridSvcDescriptor != null) services.Remove(hybridSvcDescriptor);
            services.AddSingleton<IHybridCacheService>(new PassthroughHybridCacheService());

            // Disable real RabbitMQ
            services.AddMassTransitTestHarness();

            // Initialize SQLite DB
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CourseCatalogDbContext>();
            db.Database.EnsureCreated();
            DatabaseSeeder.Seed(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _keepAliveConnection.Dispose();
    }
}