namespace StudentPortal.IdentityServer.Data;

using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using StudentPortal.IdentityServer.Entities;
using StudentPortal.IdentityServer.Configuration; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var identityDb = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var configDb = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        var persistedGrantDb = serviceProvider.GetRequiredService<PersistedGrantDbContext>();
        
        Console.WriteLine("Applying IdentityServer database migrations...");
        
        await identityDb.Database.MigrateAsync();
        await configDb.Database.MigrateAsync();
        await persistedGrantDb.Database.MigrateAsync();
        
        Console.WriteLine("Database migrations completed.");

        if (!await configDb.Clients.AnyAsync())
        {
            Console.WriteLine("Seeding clients...");
            foreach (var client in Config.Clients)
            {
                await configDb.Clients.AddAsync(client.ToEntity());
            }
            await configDb.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("Clients already seeded.");
        }

        if (!await configDb.IdentityResources.AnyAsync())
        {
            Console.WriteLine("Seeding Identity Resources...");
            foreach (var resource in Config.IdentityResources)
            {
                await configDb.IdentityResources.AddAsync(resource.ToEntity());
            }
            await configDb.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("Identity Resources already seeded.");
        }

        if (!await configDb.ApiScopes.AnyAsync())
        {
            Console.WriteLine("Seeding API Scopes...");
            foreach (var scope in Config.ApiScopes)
            {
                await configDb.ApiScopes.AddAsync(scope.ToEntity());
            }
            await configDb.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("API Scopes already seeded.");
        }
        
        if (!await configDb.ApiResources.AnyAsync())
        {
            Console.WriteLine("Seeding API Resources...");
            foreach (var resource in Config.ApiResources)
            {
                await configDb.ApiResources.AddAsync(resource.ToEntity());
            }
            await configDb.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("API Resources already seeded.");
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            Console.WriteLine("Created role: Admin");
        }
        
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            Console.WriteLine("Created role: User");
        }

        var adminEmail = "admin@studentportal.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "Admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(admin, "Admin@1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                Console.WriteLine($"Created admin user: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine("Admin user already exists.");
        }
        
        var userEmail = "student@studentportal.com";
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "Student",
                Email = userEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(user, "User@1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                Console.WriteLine($"Created regular user: {userEmail}");
            }
            else
            {
                Console.WriteLine($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine("Regular user already exists.");
        }
        
        Console.WriteLine("Seed data completed successfully.");
    }
}