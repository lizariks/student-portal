namespace StudentPortal.AppHost.Extensions;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

public static class KeycloakExtensions
{
    public static IResourceBuilder<T> WithKeycloakEnvironment<T>(
        this IResourceBuilder<T> builder,
        EndpointReference keycloakUrl,
        string realm,
        string audience) where T : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("Authentication__Schemes__Bearer__Authority", $"{keycloakUrl}/realms/{realm}")
            .WithEnvironment("Authentication__Schemes__Bearer__Audience", audience)
            .WithEnvironment("Authentication__Schemes__Bearer__RequireHttpsMetadata", "false")
            .WithEnvironment("Authentication__Schemes__Bearer__ValidateAudience", "true")
            .WithEnvironment("Keycloak__Url", keycloakUrl)
            .WithEnvironment("Keycloak__Realm", realm);
    }
}