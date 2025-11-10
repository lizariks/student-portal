namespace StudentPortal.ServiceDefaults.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


    public static class GrpcServiceExtensions
    {
        public static IServiceCollection AddGrpcWithObservability(this IServiceCollection services, IHostEnvironment? env = null)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcLoggingInterceptor>();
                options.EnableDetailedErrors = env?.IsDevelopment() ?? false;

                options.MaxReceiveMessageSize = 16 * 1024 * 1024;
                options.MaxSendMessageSize = 16 * 1024 * 1024;
            });
            services.AddSingleton<GrpcLoggingInterceptor>();
            if (env?.IsDevelopment() ?? false)
            {
                services.AddGrpcReflection();
            }

            return services;
        }

        public static WebApplication MapGrpcServicesWithReflection(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            return app;
        }
    }