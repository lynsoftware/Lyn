using Lyn.Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Lyn.Backend.Data;

public class DatabaseSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
    
    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "User" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }
    
    private async Task SeedAdminUserAsync()
    {
        // Hent admin credentials fra configuration (appsettings eller environment variables)
        var adminEmail = configuration["AdminUser:Email"] ?? "dev@lynsoftware.com";
        var adminPassword = configuration["AdminUser:Password"];
        
        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin password must be configured in production environment");
        }
        
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user. Errors: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
        }
    }
}