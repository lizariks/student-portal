
using  StudentPortal.ServiceDefaults.MiddleWare;
using Microsoft.AspNetCore.Builder;
namespace StudentPortal.ServiceDefaults.Extensions;


    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
