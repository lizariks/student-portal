namespace StudentPortal.ServiceDefaults.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;


    public static class SwaggerExtensions
    {
        /// <summary>
        /// Adds Swagger with Keycloak OAuth2 authentication support.
        /// </summary>
        public static IServiceCollection AddSwaggerWithKeycloak(
            this IServiceCollection services,
            IConfiguration configuration,
            string title,
            string version = "v1")
        {
            var keycloakUrl =
                configuration["Keycloak:Url"]
                ?? configuration["services:keycloak:https:0"]
                ?? configuration["services:keycloak:http:0"]
                ?? Environment.GetEnvironmentVariable("KEYCLOAK_URL")
                ?? "http://localhost:8080";

            var realm =
                configuration["Keycloak:Realm"]
                ?? Environment.GetEnvironmentVariable("KEYCLOAK_REALM")
                ?? "StudentPortal";

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = title,
                    Version = version,
                    Description = $"StudentPortal API authenticated via Keycloak (Realm: {realm})",
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "OAuth2 Authorization Code Flow with PKCE via Keycloak",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri($"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token"),

                            Scopes = new Dictionary<string, string>
                            {
                                // OpenID Cnect standard scopes
                                { "openid", "OpenID Connect scope" },
                                { "profile", "User profile information" },
                                { "email", "User email address" },

                                { "studentportal_api", "Full access to StudentPortal APIs" },

                                { "catalog:read", "Read the catalog" },
                                { "catalog:manage", "Manage the catalog" },
                                { "catalog:delete", "Delete the catalog" },

                                { "enrollment:read", "Read user's enrollments" },
                                { "enrollment:write", "Create new enrollments" },
                                { "enrollment:delete", "Delete existing enrollments" },

                                { "discussion:read", "Read discussion threads" },
                                { "discussion:write", "Create new discussions" },
                                { "discussion:delete", "Delete discussion threads" },
                            }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[]
                        {
                            "openid",
                            "profile",
                            "studentportal_api",
                            "catalog:read",
                            "catalog:manage",
                            "enrollment:read",
                            "enrollment:create",
                            "enrollment:delete",
                            "discussion:read",
                            "discussion:write",
                            "discussion:delete"
                        }
                    }
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static WebApplication UseSwaggerWithKeycloak(
            this WebApplication app,
            IConfiguration? configuration = null)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json",
                        $"{app.Environment.ApplicationName} v1");

                    options.OAuthClientId("swagger");
                    options.OAuthAppName("StudentPortal Swagger UI");
                    options.OAuthUsePkce();

                    options.OAuthScopes(
                        "openid",
                        "profile",
                        "email",
                        "studentportal_api"
                    );

                    options.OAuthScopeSeparator(" ");

                    options.EnablePersistAuthorization();

                    options.DisplayRequestDuration();
                });
            }

            return app;
        }
    }