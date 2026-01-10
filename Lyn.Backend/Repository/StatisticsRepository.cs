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
    
    // See interface for summary
    public async Task IncrementWindowsDownloadAsync()
    {
        var stats = await context.PasswordGeneratorUsageStatistics.FindAsync(1);
        if (stats != null)
        {
            
            stats.WindowsDownloads++;
            await context.SaveChangesAsync();
        }
    }
    
    // See interface for summary
    public async Task IncrementApkDownloadAsync()
    {
        var stats = await context.PasswordGeneratorUsageStatistics.FindAsync(1);
        if (stats != null)
        {
            
            stats.ApkDownloads++;
            await context.SaveChangesAsync();
        }
    }
}