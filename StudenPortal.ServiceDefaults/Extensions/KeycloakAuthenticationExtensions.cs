namespace StudentPortal.ServiceDefaults.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


    public static class KeycloakAuthenticationExtensions
    {
        public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var realm =
                configuration["Keycloak:Realm"]
                ?? Environment.GetEnvironmentVariable("KEYCLOAK_REALM")
                ?? "StudentPortal";

            var audience =
                configuration["Keycloak:Audience"]
                ?? Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE")
                ?? "studentportal_api";

            var keycloakUrl =
                configuration["Keycloak:Url"]
                ?? configuration["services:keycloak:https:0"]
                ?? configuration["services:keycloak:http:0"]
                ?? Environment.GetEnvironmentVariable("KEYCLOAK_URL")
                ?? "http://localhost:8080";

            services
                .AddAuthentication()
                .AddKeycloakJwtBearer("keycloak",
                    realm: realm,
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.Audience = audience;

                        options.Authority =
                            $"{keycloakUrl.TrimEnd('/')}/realms/{realm}";
                    });

            return services;
        }
    }
