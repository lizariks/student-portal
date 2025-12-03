namespace StudentPortal.ServiceDefaults.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;


    /// <summary>
    /// Custom authorization attribute for permission-based access control.
    /// Usage: [RequirePermission("orders:create")]
    /// </summary>
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }

    /// <summary>
    /// Authorization requirement for permission-based access control.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }

    /// <summary>
    /// Authorization handler that validates permissions from Keycloak JWT tokens.
    /// Checks the "scope" claim which contains space-separated permissions.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Extract the scope claim from JWT token
            // Keycloak includes permissions in the "scope" claim as space-separated values
            var scopeClaim = context.User.FindFirst("scope")?.Value;

            if (string.IsNullOrEmpty(scopeClaim))
            {
                // No scope claim found - access denied
                return Task.CompletedTask;
            }

            // Split scopes by space
            var scopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Check if user has the required permission
            if (scopes.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension methods for configuring permission-based authorization.
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// Adds permission-based authorization to the service collection.
        /// This enables the use of [RequirePermission] attribute on controllers/endpoints.
        /// </summary>
        public static IServiceCollection AddPermissionAuthorization(
            this IServiceCollection services)
        {
            // Register the permission handler
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Add authorization policies for common permissions
            services.AddAuthorizationBuilder()

                .AddPolicy("Permission:catalog:read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("catalog:read")))
                .AddPolicy("Permission:catalog:manage", policy =>
                    policy.Requirements.Add(new PermissionRequirement("catalog:manage")))

                .AddPolicy("Permission:enrollment:read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("enrollment:read")))
                .AddPolicy("Permission:enrollment:create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("enrollment:create")))
                .AddPolicy("Permission:enrollment:delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("enrollment:delete")))

                .AddPolicy("Permission:discussion:read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("discussion:read")))
                .AddPolicy("Permission:discussion:write", policy =>
                    policy.Requirements.Add(new PermissionRequirement("discussion:write")))
                .AddPolicy("Permission:discussion:write", policy =>
                    policy.Requirements.Add(new PermissionRequirement("discussion:write")));

            return services;
        }
        
        public static IServiceCollection AddPermissionPolicy(
            this IServiceCollection services,
            string permission)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy($"Permission:{permission}", policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));

            return services;
        }
    }