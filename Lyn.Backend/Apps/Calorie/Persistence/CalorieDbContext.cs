using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Apps.Calorie.Persistence;

/// <summary>
/// Egen bounded context for Calorie-produktet. Deler samme Postgres-database
/// som AppDbContext, men har egen migrasjonshistorikk (__EFMigrationsHistory_Calorie)
/// slik at de to kontekstene versjoneres uavhengig.
/// </summary>
public class CalorieDbContext(DbContextOptions<CalorieDbContext> options) : DbContext(options)
{
    public DbSet<FoodProduct> FoodProducts { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealIngredient> MealIngredients { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<LogEntryIngredient> LogEntryIngredients { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<MealBudget> MealBudgets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Register all Entity Configuration files related to Calorie app.
        // Predikatet kjøres på ALLE konstruerbare typer (bl.a. Program i global namespace der
        // Namespace er null), så vi må null-sjekke — ellers NRE ved oppstart.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CalorieDbContext).Assembly,
            t => t.Namespace?.Contains(".Apps.Calorie.") == true);
    }
    
    
}