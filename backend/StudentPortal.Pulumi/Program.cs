using Pulumi;
using System.Collections.Generic;
using Keycloak = Pulumi.Keycloak;
using System.Linq;

return await Deployment.RunAsync(() =>
{
    var config = new Pulumi.Config("studentportal");
    var realmName = config.Require("realmName"); 

    var realm = new Keycloak.Realm("studentportal-realm", new Keycloak.RealmArgs
    {
        RealmName = realmName, 
        Enabled = true,
        DisplayName = "Student Portal Platform",
        RegistrationAllowed = true,
        RegistrationEmailAsUsername = true,
        VerifyEmail = true, 
        LoginWithEmailAllowed = true,
        ResetPasswordAllowed = true,
        RememberMe = true,
        SsoSessionIdleTimeout = "30m",
        SsoSessionMaxLifespan = "10h",
        AccessTokenLifespan = "15m", 
        AccessTokenLifespanForImplicitFlow = "30m",
    });

    var adminRole = new Keycloak.Role("admin-role", new Keycloak.RoleArgs
    {
        RealmId = realm.Id,
        Name = "admin",
        Description = "Administrator with full platform access",
    });

    var userRole = new Keycloak.Role("user-role", new Keycloak.RoleArgs
    {
        RealmId = realm.Id,
        Name = "user",
        Description = "Standard Student/User role",
    });

    var moderatorRole = new Keycloak.Role("moderator-role", new Keycloak.RoleArgs
    {
        RealmId = realm.Id,
        Name = "moderator",
        Description = "Moderator for discussion forums",
    });

    var catalogReadScope = new Keycloak.OpenId.ClientScope("catalog-read-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "catalog.read",
        Description = "Read access to course catalog",
        IncludeInTokenScope = true,
    });
    var catalogManageScope = new Keycloak.OpenId.ClientScope("catalog-manage-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "catalog.manage",
        Description = "Create/Update/Delete courses in catalog",
        IncludeInTokenScope = true,
    });

    var enrollmentReadScope = new Keycloak.OpenId.ClientScope("enrollment-read-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "enrollment.read",
        Description = "Read enrollment status",
        IncludeInTokenScope = true,
    });
    var enrollmentManageScope = new Keycloak.OpenId.ClientScope("enrollment-manage-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "enrollment.manage",
        Description = "Manage user enrollment",
        IncludeInTokenScope = true,
    });

    var discussionReadScope = new Keycloak.OpenId.ClientScope("discussion-read-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "discussion.read",
        Description = "Read forum posts",
        IncludeInTokenScope = true,
    });
    var discussionWriteScope = new Keycloak.OpenId.ClientScope("discussion-write-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "discussion.write",
        Description = "Create and update posts",
        IncludeInTokenScope = true,
    });
    var discussionDeleteScope = new Keycloak.OpenId.ClientScope("discussion-delete-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "discussion.delete",
        Description = "Delete any posts (moderator level)",
        IncludeInTokenScope = true,
    });

    var portalApiScope = new Keycloak.OpenId.ClientScope("portal-api-scope", new Keycloak.OpenId.ClientScopeArgs
    {
        RealmId = realm.Id,
        Name = "studentportal_api",
        Description = "General Student Portal API Access",
        IncludeInTokenScope = true,
    });

    var audienceMapper = new Keycloak.OpenId.AudienceProtocolMapper("api-audience-mapper", new Keycloak.OpenId.AudienceProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientScopeId = portalApiScope.Id, 
        Name = "api-audience",
        IncludedClientAudience = "studentportal_api", 
        AddToAccessToken = true,
    });

    var roleMapper = new Keycloak.OpenId.UserRealmRoleProtocolMapper("realm-role-mapper", new Keycloak.OpenId.UserRealmRoleProtocolMapperArgs
    {
        RealmId = realm.Id,
        ClientScopeId = portalApiScope.Id,
        Name = "realm-role-mapper",
        ClaimName = "role",
        ClaimValueType = "String",
        AddToAccessToken = true,
        Multivalued = true,
    });


    var swaggerRedirectUris = new[]
    {
        "https://localhost:7049/swagger/oauth2-redirect.html",
        "https://localhost:7048/swagger/oauth2-redirect.html",
        "https://localhost:7047/swagger/oauth2-redirect.html",
        "https://localhost:7050/swagger/oauth2-redirect.html",
        "https://localhost:5000/swagger/oauth2-redirect.html",
    };

    // 5.1. SWAGGER CLIENT (Public Client з PKCE)
    var swaggerClient = new Keycloak.OpenId.Client("swagger-client", new Keycloak.OpenId.ClientArgs
    {
        RealmId = realm.Id,
        ClientId = "swagger",
        Name = "Swagger UI / SPA Client",
        Enabled = true,
        AccessType = "PUBLIC",
        StandardFlowEnabled = true, 
        DirectAccessGrantsEnabled = false,
        ImplicitFlowEnabled = false,
        ValidRedirectUris = swaggerRedirectUris.ToArray(),
        WebOrigins = new[] { "*" },
        PkceCodeChallengeMethod = "S256", 
    });

    var swaggerDefaultScopes = new Keycloak.OpenId.ClientDefaultScopes("swagger-default-scopes", new Keycloak.OpenId.ClientDefaultScopesArgs
    {
        RealmId = realm.Id,
        ClientId = swaggerClient.Id,
        DefaultScopes = new InputList<string>
        {
            "openid",
            "profile",
            "email",
            portalApiScope.Name, 
        },
    });

    var swaggerOptionalScopes = new Keycloak.OpenId.ClientOptionalScopes("swagger-optional-scopes", new Keycloak.OpenId.ClientOptionalScopesArgs
    {
        RealmId = realm.Id,
        ClientId = swaggerClient.Id,
        OptionalScopes = new InputList<string>
        {
            catalogManageScope.Name, 
            discussionWriteScope.Name,
            enrollmentManageScope.Name
        },
    });


    var frontendClient = new Keycloak.OpenId.Client("frontend-client", new Keycloak.OpenId.ClientArgs
    {
        RealmId = realm.Id,
        ClientId = "studentportal-frontend",
        Name = "Student Portal React Frontend",
        Enabled = true,
        AccessType = "PUBLIC",
        StandardFlowEnabled = true,
        DirectAccessGrantsEnabled = true,
        ImplicitFlowEnabled = false,
        ValidRedirectUris = new[] { "http://localhost:5173/*" },
        WebOrigins = new[] { "http://localhost:5173" },
        PkceCodeChallengeMethod = "S256",
    });

    var frontendDefaultScopes = new Keycloak.OpenId.ClientDefaultScopes("frontend-default-scopes", new Keycloak.OpenId.ClientDefaultScopesArgs
    {
        RealmId = realm.Id,
        ClientId = frontendClient.Id,
        DefaultScopes = new InputList<string> { "openid", "profile", "email", portalApiScope.Name },
    });

    var frontendOptionalScopes = new Keycloak.OpenId.ClientOptionalScopes("frontend-optional-scopes", new Keycloak.OpenId.ClientOptionalScopesArgs
    {
        RealmId = realm.Id,
        ClientId = frontendClient.Id,
        OptionalScopes = new InputList<string> { catalogManageScope.Name, discussionWriteScope.Name, enrollmentManageScope.Name },
    });

    var postmanClient = new Keycloak.OpenId.Client("postman-client", new Keycloak.OpenId.ClientArgs
    {
        RealmId = realm.Id,
        ClientId = "postman",
        Name = "Postman Testing Client",
        Enabled = true,
        AccessType = "CONFIDENTIAL", 
        StandardFlowEnabled = true, 
        ImplicitFlowEnabled = false,
        ServiceAccountsEnabled = false,
        ClientSecret = "postman-secret-123", 
        ValidRedirectUris = new[] { "https://oauth.pstmn.io/v1/callback" }
    });


    var apiClient = new Keycloak.OpenId.Client("api-client", new Keycloak.OpenId.ClientArgs
    {
        RealmId = realm.Id,
        ClientId = "studentportal_api_service",
        Name = "Student Portal API Service Account",
        Enabled = true,
        AccessType = "CONFIDENTIAL",
        StandardFlowEnabled = false, 
        ServiceAccountsEnabled = true, 
        ClientSecret = "portal-api-secret-change-me",
    });

    // ===== 6. ТЕСТОВІ КОРИСТУВАЧІ =====

    // Створюємо Admin
    var adminUser = new Keycloak.User("admin-user", new Keycloak.UserArgs
    {
        RealmId = realm.Id,
        Username = "admin@portal.local",
        Email = "admin@portal.local",
        EmailVerified = true,
        Enabled = true,
        FirstName = "Portal",
        LastName = "Admin",
        InitialPassword = new Keycloak.Inputs.UserInitialPasswordArgs
        {
            Value = "Admin@1234",
            Temporary = false,
        },
    });

    var adminUserRoles = new Keycloak.UserRoles("admin-user-roles", new Keycloak.UserRolesArgs
    {
        RealmId = realm.Id,
        UserId = adminUser.Id,
        RoleIds = new[] { adminRole.Id, moderatorRole.Id },
    });

    // Створюємо Стандартного Студента
    var regularUser = new Keycloak.User("regular-user", new Keycloak.UserArgs
    {
        RealmId = realm.Id,
        Username = "student@portal.local",
        Email = "student@portal.local",
        EmailVerified = true,
        Enabled = true,
        FirstName = "Regular",
        LastName = "Student",
        InitialPassword = new Keycloak.Inputs.UserInitialPasswordArgs
        {
            Value = "User@1234",
            Temporary = false,
        },
    });

    var regularUserRoles = new Keycloak.UserRoles("regular-user-roles", new Keycloak.UserRolesArgs
    {
        RealmId = realm.Id,
        UserId = regularUser.Id,
        RoleIds = new[] { userRole.Id },
    });


    return new Dictionary<string, object?>
    {
        ["RealmName"] = realm.RealmName,
        ["RealmUrl"] = Output.Format($"http://localhost:8080/realms/{realm.RealmName}"),
        ["SwaggerClientId"] = swaggerClient.ClientId,
        ["ApiClientId"] = apiClient.ClientId,
        ["FrontendClientId"] = frontendClient.ClientId,
        ["TestUsers"] = @"
        Admin:   admin@portal.local / Admin@1234
        Student: student@portal.local / User@1234
        ",
    };
});