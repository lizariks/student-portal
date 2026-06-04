using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace StudentPortal.ServiceDefaults.Redis;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnectionString = configuration.GetConnectionString("redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new InvalidOperationException("Redis connection string 'redis' is not configured.");

        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.AbortOnConnectFail = false;
        options.ConnectRetry = 5;
        options.ConnectTimeout = 5000;
        options.SyncTimeout = 5000;

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(options));

        services.AddStackExchangeRedisCache(cacheOptions =>
        {
            cacheOptions.Configuration = redisConnectionString;
            cacheOptions.InstanceName = "StudentPortal:";
        });

        services.AddHealthChecks()
            .AddRedis(redisConnectionString, name: "redis", timeout: TimeSpan.FromSeconds(3));

        return services;
    }
}