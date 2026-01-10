using Lyn.Backend.Models;

namespace Lyn.Backend.Repository;

public interface IDownloadRepository
{
    /// <summary>
    /// Gets a specific file by Id, as long as it is active
    /// </summary>
    /// <param name="id">ID of file</param>
    /// <returns>The file entity as AppDownload</returns>
    Task<AppDownload?> DownloadFileAsync(int id);
    
    /// <summary>
    /// Get the Ids of all the latest and active files
    /// </summary>
    /// <returns></returns>
    Task<List<AppDownload>> GetLatestAsync();

    /// <summary>
    /// Adds a new file to download
    /// </summary>
    /// <param name="download">The file to add</param>
    Task AddAsync(AppDownload download);
    
    /// <summary>
    /// Checks if any downloads exists
    /// </summary>
    Task<bool> AppDownloadsExistAsync();

    Task SaveChangesAsync();
}