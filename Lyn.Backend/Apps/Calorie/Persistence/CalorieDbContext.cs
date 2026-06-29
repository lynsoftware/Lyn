using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Apps.Calorie.Persistence;

// Egen bounded context for Calorie-produktet. Deler samme Postgres-database
// som AppDbContext, men har egen migrasjonshistorikk (__EFMigrationsHistory_Calorie)
// slik at de to kontekstene versjoneres uavhengig.
public class CalorieDbContext(DbContextOptions<CalorieDbContext> options) : DbContext(options)
{
    // DbSets legges til her etter hvert som domenemodellene godkjennes
    // (Ingredient, Meal, LogEntry ...). Tom forelopig.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}