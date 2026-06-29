using Lyn.Backend.Infrastructure.Persistence;

namespace Lyn.Backend.Apps.PasswordGenerator.Repositories;

public class PasswordGeneratorStatisticsRepository(AppDbContext context) : IPasswordGeneratorStatisticsRepository
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