namespace StudentPortal.IdentityService.DAL.Data;

using StudentPortal.IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class IdentitySeeder
{
    private static class SeedGuids
    {
        public static readonly Guid AdminUserId = new("8e02d45a-3511-477f-a6e5-4a571f337a89");
        public static readonly Guid JohnDoeUserId = new("9a45b1c2-3e4f-5678-a90b-123456789012");
        public static readonly Guid AliceWonderUserId = new("0c3d2e1f-4a5b-6789-b0c1-234567890123");
        public static readonly Guid MarkSmithUserId = new("1d2c3b4a-5e6f-7890-c1d2-345678901234");
    }

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        logger.LogInformation("Applying pending migrations for IdentityDbContext...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "User", "Student", "Teacher"];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
                logger.LogInformation("Created role **{Role}**", roleName);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var adminId = SeedGuids.AdminUserId;
        const string adminEmail = "admin@studentportal.com";
        const string adminUsername = "AdminSP";
        const string adminPassword = "Admin@1234";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                Id = adminId,
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Created admin user: **{Email}**", adminEmail);
            }
            else
            {
                logger.LogWarning("Failed to create admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        var testUsers = new List<(Guid Id, string Email, string Username, string Role)>
        {
            (SeedGuids.JohnDoeUserId,      "john@studentportal.com",  "JohnDoe",     "Student"),
            (SeedGuids.AliceWonderUserId,  "alice@studentportal.com", "AliceWonder", "Student"),
            (SeedGuids.MarkSmithUserId,    "mark@studentportal.com",  "MarkSmith",   "Teacher")
        };

        const string userPassword = "User@1234";

        foreach (var (id, email, username, role) in testUsers)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                // Ensure correct role is assigned even if user was already seeded
                if (!await userManager.IsInRoleAsync(existing, role))
                {
                    await userManager.AddToRoleAsync(existing, role);
                    logger.LogInformation("Assigned role {Role} to existing user {Email}", role, email);
                }
                continue;
            }

            var user = new ApplicationUser
            {
                Id = id,
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, userPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("Created test user: **{Email}** with role {Role}", email, role);
            }
            else
            {
                logger.LogWarning("Failed to create test user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}