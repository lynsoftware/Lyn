using Lyn.Backend.Apps.PasswordGenerator.Models;
using Lyn.Backend.Platform.AppReleases.Models;
using Lyn.Backend.Platform.Auth.Models;
using Lyn.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Website and password generator app
/// </summary>
/// <param name="options"></param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    // ============================================== DBSETS ==============================================
    // Identity tabeller vi får fra IdentityDbContext
    // - Users (ApplicationUser)
    // - Roles
    // - UserRoles
    // - UserClaims
    // - UserLogins
    // - UserTokens
    // - RoleClaims

    public DbSet<PasswordGeneratorUsageStatistic> PasswordGeneratorUsageStatistics { get; set; }
    public DbSet<AppRelease> AppReleases { get; set; }
    
    public DbSet<SupportTicket> SupportTickets { get; set; }
    
    public DbSet<SupportAttachment> SupportAttachments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    
    {
        base.OnModelCreating(modelBuilder);
        
        // Register all the Entity Configuration files that does not belong for the Calorie app.
        // Predikatet kjøres på ALLE konstruerbare typer i assemblyet (bl.a. Program i global
        // namespace der Namespace er null), så vi må null-sjekke — ellers NRE ved oppstart.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly,
            t => t.Namespace?.Contains(".Apps.Calorie.") != true);

        // Seed initial statistic
        modelBuilder.Entity<PasswordGeneratorUsageStatistic>().HasData(
            new PasswordGeneratorUsageStatistic 
            { 
                Id = 1, 
                PasswordsGenerated = 0, 
                WindowsDownloads = 0, 
                ApkDownloads = 0 
            }
        );
    }
}