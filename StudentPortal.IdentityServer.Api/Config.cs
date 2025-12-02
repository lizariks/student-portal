using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace StudentPortal.IdentityServer.Configuration;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),    
            new IdentityResources.Profile(),   
            new IdentityResources.Email(),     
            new IdentityResource("roles", "User Roles", new [] { "role" }) 
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new[]
        {
            // Course Catalog Scopes
            new ApiScope("catalog.read", "Read access to course information"),
            new ApiScope("catalog.manage", "Manage (Create/Update/Delete) courses"),

            // Enrollment Scopes
            new ApiScope("enrollment.read", "Read access to enrollment status and progress"),
            new ApiScope("enrollment.manage", "Enroll/Unenroll users and manage progress"),

            // Discussion Scopes
            new ApiScope("discussion.read", "Read access to forum posts and comments"),
            new ApiScope("discussion.write", "Create and edit posts/comments"),
            new ApiScope("discussion.delete", "Delete any posts/comments (Moderator/Admin)")
        };
    // ApiResource об'єднує набір Scopes під один сервіс.
    public static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource("course_catalog_api", "Course Catalog Service")
            {
                Scopes = { "catalog.read", "catalog.manage" }, 
                UserClaims = { "role", "email" }
            },
            new ApiResource("enrollment_api", "Enrollment Service")
            {
                Scopes = { "enrollment.read", "enrollment.manage" },
                UserClaims = { "role", "email" }
            },
            new ApiResource("discussion_api", "Discussion Service")
            {
                Scopes = { "discussion.read", "discussion.write", "discussion.delete" },
                UserClaims = { "role", "email" }
            }
        };
        
    public static IEnumerable<Client> Clients =>
        new[]
        {
            new Client
            {
                ClientId = "aggregator_service",
                ClientName = "Service Aggregator (S2S)",
                AllowedGrantTypes = GrantTypes.ClientCredentials, 
                ClientSecrets = { new Secret("aggregator_portal_secret".Sha256()) },
                AccessTokenLifetime = 3600,

                AllowedScopes = { "catalog.read", "enrollment.read", "discussion.read" } 
            },

            new Client
            {
                ClientId = "swagger",
                ClientName = "Swagger UI",
                AllowedGrantTypes = GrantTypes.Code, 
                RequirePkce = true, 
                RequireClientSecret = false, 
                AllowAccessTokensViaBrowser = true,
                AllowOfflineAccess = true, 
                AccessTokenLifetime = 3600,
                RedirectUris = 
                { 
                    "https://localhost:7049/swagger/oauth2-redirect.html",
                    "https://localhost:7048/swagger/oauth2-redirect.html",
                    "https://localhost:7047/swagger/oauth2-redirect.html",
                    "https://localhost:7050/swagger/oauth2-redirect.html",
                    "https://localhost:5000/swagger/oauth2-redirect.html",
                },
                AllowedCorsOrigins = 
                { 
                    "https://localhost:7049",
                    "https://localhost:7048",
                    "https://localhost:7047",
                    "https://localhost:7050",
                    "https://localhost:5000" 
                },

                AllowedScopes = { 
                    "openid", 
                    "profile", 
                    "email", 
                    "catalog.read", 
                    "enrollment.manage", 
                    "discussion.write" 
                }
            }
        };
}