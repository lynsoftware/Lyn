using Lyn.Backend.Data;

namespace Lyn.Backend.Repository;

public class StatisticsRepository(AppDbContext context) : IStatisticsRepository
{   
    // See interface for summary
    public async Task IncrementPasswordGeneratedAsync()
    {
        var stats = await context.PasswordGeneratorUsageStatistics.FindAsync(1);
        if (stats != null)
        {
            
            stats.PasswordsGenerated++;
            await context.SaveChangesAsync();
        }
    }
}