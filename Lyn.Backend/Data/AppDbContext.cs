using Lyn.Backend.Models;
using Lyn.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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
    public DbSet<AppDownload> AppDownloads { get; set; }
    
    public DbSet<SupportTicket> SupportTickets { get; set; }
    
    public DbSet<SupportAttachment> SupportAttachments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    
    {
        base.OnModelCreating(modelBuilder);
        
        // Bytes for FileData
        modelBuilder.Entity<AppDownload>()
            .Property(e => e.FileData)
            .HasColumnType("bytea"); 
        
        
        // ==================== SupportTicket ====================
        modelBuilder.Entity<SupportTicket>()
            .HasMany(e => e.Attachments)
            .WithOne(e => e.SupportTicket)
            .HasForeignKey(e => e.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
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