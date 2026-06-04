namespace StudentPortal.ServiceDefaults.Extensions;

using StudentPortal.ServiceDefaults.Handlers;
using Microsoft.Extensions.DependencyInjection;


    public static class CorrelationIdForwardingExtensions
    {
        public static IServiceCollection AddCorrelationIdForwarding(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<CorrelationIdHandler>();

            return services;
        }

        public static IHttpClientBuilder AddCorrelationIdHttpClient(
            this IServiceCollection services,
            string name,
            Action<HttpClient>? configureClient = null)
        {
            var builder = services.AddHttpClient(name, configureClient!);
            builder.AddHttpMessageHandler<CorrelationIdHandler>();
            return builder;
        }

        public static IHttpClientBuilder AddCorrelationIdHttpClient<TClient>(
            this IServiceCollection services,
            Action<HttpClient>? configureClient = null)
            where TClient : class
        {
            var builder = services.AddHttpClient<TClient>(configureClient!);
            builder.AddHttpMessageHandler<CorrelationIdHandler>();
            return builder;
        }
    }
