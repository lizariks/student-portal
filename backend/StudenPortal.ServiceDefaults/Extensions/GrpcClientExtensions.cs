namespace StudentPortal.ServiceDefaults.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public static class GrpcClientExtensions
{
    public static IHttpClientBuilder AddGrpcClientWithDefaults<TClient>(
        this IServiceCollection services,
        string serviceUri,
        IHostEnvironment environment)  
        where TClient : class
    {
        var builder = services.AddGrpcClient<TClient>(options =>
            {
                options.Address = new Uri(serviceUri);
            })
            .ConfigureChannel(channelOptions =>
            {
                channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
                channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
            });

        if (environment.IsDevelopment() && serviceUri.StartsWith("https"))
        {
            builder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = 
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return handler;
            });
        }

        return builder
            .AddServiceDiscovery()
            .AddGrpcResilienceHandler(ResilienceProfile.Standard);
    }
}