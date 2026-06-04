namespace StudentPortal.ServiceDefaults.Extensions;

using StudentPortal.ServiceDefaults.Background;
using StudentPortal.ServiceDefaults.Background.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMqConsumer<TEvent, TConsumer>(this IServiceCollection services)
        where TEvent : class
        where TConsumer : class, IConsumer<TEvent> 
    {
        services.AddScoped<TConsumer>();
        
        services.AddHostedService<RabbitMqConsumerService<TEvent, TConsumer>>();
        
        return services;
    }
}