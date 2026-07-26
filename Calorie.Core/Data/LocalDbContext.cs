using Calorie.Core.Features.DailyLog;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Features.Goals;
using Calorie.Core.Features.Goals.Models;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Calorie.Core.Data;

/// <summary>
/// Lokal SQLite-database på enheten — sannheten i lokal-først-arkitekturen.
/// Én fil i appens sandkasse; migreres ved oppstart via LocalDb.Initialize.
/// </summary>
public class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
{
    public DbSet<LibraryIngredient> Ingredients { get; set; }
    public DbSet<LibraryMeal> Meals { get; set; }
    public DbSet<MealComponent> MealComponents { get; set; }
    public DbSet<LogEntryRecord> LogEntries { get; set; }
    public DbSet<UserGoalRecord> Goals { get; set; }
    public DbSet<MealBudgetRecord> MealBudgets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Måltid eier komponentene; en ingrediens i bruk kan ikke slettes
        modelBuilder.Entity<LibraryMeal>()
            .HasMany(m => m.Components)
            .WithOne()
            .HasForeignKey(c => c.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MealComponent>()
            .HasOne(c => c.Ingredient)
            .WithMany()
            .HasForeignKey(c => c.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dagsloggen spørres alltid per lokal dato
        modelBuilder.Entity<LogEntryRecord>()
            .HasIndex(e => e.LoggedDate);

        // Målet eier budsjettene; maks ett budsjett per måltidstype per mål
        modelBuilder.Entity<UserGoalRecord>()
            .HasMany(g => g.MealBudgets)
            .WithOne()
            .HasForeignKey(b => b.UserGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MealBudgetRecord>()
            .HasIndex(b => new { b.UserGoalId, b.MealType })
            .IsUnique();

        modelBuilder.Entity<UserGoalRecord>()
            .HasIndex(g => g.EffectiveFromDate);
    }
}
