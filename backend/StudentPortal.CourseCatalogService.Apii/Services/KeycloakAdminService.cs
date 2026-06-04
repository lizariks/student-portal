namespace StudentPortal.CourseCatalogService.Apii.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public record KeycloakUserRepresentation(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("createdTimestamp")] long CreatedTimestamp
);

public record KeycloakRoleRepresentation(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

public record AdminUserDto(
    string Id,
    string UserName,
    string Email,
    List<string> Roles,
    string CreatedAt
);

public class KeycloakAdminService
{
    private readonly IHttpClientFactory _factory;
    private readonly string _keycloakUrl;
    private readonly string _realm;
    private readonly string _adminUsername;
    private readonly string _adminPassword;

    public KeycloakAdminService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _keycloakUrl = (
            configuration["Keycloak:Url"]
            ?? configuration["services:keycloak:https:0"]
            ?? configuration["services:keycloak:http:0"]
            ?? "http://localhost:8080"
        ).TrimEnd('/');
        _realm = configuration["Keycloak:Realm"] ?? "StudentPortal";
        _adminUsername = configuration["Keycloak:AdminUsername"] ?? "admin";
        _adminPassword = configuration["Keycloak:AdminPassword"] ?? "admin";
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsync(
            $"{_keycloakUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = _adminUsername,
                ["password"] = _adminPassword
            }));
        res.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var users = await client.GetFromJsonAsync<List<KeycloakUserRepresentation>>(
            $"{_keycloakUrl}/admin/realms/{_realm}/users?max=200") ?? [];

        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await client.GetFromJsonAsync<List<KeycloakRoleRepresentation>>(
                $"{_keycloakUrl}/admin/realms/{_realm}/users/{user.Id}/role-mappings/realm") ?? [];

            result.Add(new AdminUserDto(
                Id: user.Id,
                UserName: user.Username,
                Email: user.Email ?? "",
                Roles: roles.Select(r => r.Name).ToList(),
                CreatedAt: DateTimeOffset.FromUnixTimeMilliseconds(user.CreatedTimestamp).UtcDateTime.ToString("o")
            ));
        }
        return result;
    }

    public async Task CreateUserAsync(string userName, string email, string password, string role)
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRes = await client.PostAsJsonAsync(
            $"{_keycloakUrl}/admin/realms/{_realm}/users",
            new
            {
                username = userName,
                email = email,
                enabled = true,
                emailVerified = true,
                credentials = new[] { new { type = "password", value = password, temporary = false } }
            });

        if (!createRes.IsSuccessStatusCode)
        {
            var body = await createRes.Content.ReadAsStringAsync();
            throw new InvalidOperationException(body);
        }

        var found = await client.GetFromJsonAsync<List<KeycloakUserRepresentation>>(
            $"{_keycloakUrl}/admin/realms/{_realm}/users?email={Uri.EscapeDataString(email)}&exact=true") ?? [];
        var userId = found.FirstOrDefault()?.Id
            ?? throw new InvalidOperationException("User created but ID could not be resolved.");

        var roleRep = await TryGetRoleAsync(client, role)
            ?? await TryGetRoleAsync(client, role.ToLower())
            ?? throw new InvalidOperationException($"Role '{role}' not found in realm '{_realm}'.");

        var assignRes = await client.PostAsJsonAsync(
            $"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm",
            new[] { roleRep });
        assignRes.EnsureSuccessStatusCode();
    }

    public async Task AddRoleAsync(string userId, string role)
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var roleRep = await TryGetRoleAsync(client, role)
            ?? await TryGetRoleAsync(client, role.ToLower())
            ?? throw new InvalidOperationException($"Role '{role}' not found.");

        (await client.PostAsJsonAsync(
            $"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm",
            new[] { roleRep })).EnsureSuccessStatusCode();
    }

    public async Task RemoveRoleAsync(string userId, string role)
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var roleRep = await TryGetRoleAsync(client, role)
            ?? await TryGetRoleAsync(client, role.ToLower())
            ?? throw new InvalidOperationException($"Role '{role}' not found.");

        var request = new HttpRequestMessage(HttpMethod.Delete,
            $"{_keycloakUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(new[] { roleRep })
        };
        (await client.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<KeycloakRoleRepresentation?> TryGetRoleAsync(HttpClient client, string roleName)
    {
        var res = await client.GetAsync(
            $"{_keycloakUrl}/admin/realms/{_realm}/roles/{Uri.EscapeDataString(roleName)}");
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>();
    }
}