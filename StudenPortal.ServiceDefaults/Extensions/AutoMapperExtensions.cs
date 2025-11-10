namespace StudentPortal.ServiceDefaults.Extensions;

using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;


    public static class AutoMapperExtensions
    {
        public static IServiceCollection AddAutoMapperWithLogging(
            this IServiceCollection services,
            Assembly assembly)
        {
            services.AddSingleton(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddMaps(assembly);
                }, loggerFactory);

                return config.CreateMapper();
            });

            return services;
        }

        public static IServiceCollection AddAutoMapperWithLogging(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            services.AddSingleton(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddMaps(assemblies);
                }, loggerFactory);

                return config.CreateMapper();
            });

            return services;
        }
    }
