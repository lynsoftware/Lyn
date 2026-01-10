namespace Lyn.Backend.Repository;

public interface IStatisticsRepository
{
    /// <summary>
    /// Increments Passwords generated
    /// </summary>
    Task IncrementPasswordGeneratedAsync();
    
    /// <summary>
    /// Increments downloads for Windows
    /// </summary>
    Task IncrementWindowsDownloadAsync();
    
    /// <summary>
    /// Increments APK downloads
    /// </summary>
    Task IncrementApkDownloadAsync();
}