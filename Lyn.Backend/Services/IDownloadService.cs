using Lyn.Backend.Models.Enums;
using Lyn.Shared.Models;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public interface IDownloadService
{
    /// <summary>
    /// Gets a file by Id and increments related statistics
    /// </summary>
    /// <param name="id">The Id for file to download</param>
    /// <returns>Result with FileDownloadDto</returns>
    Task<Result<FileDownloadDto>> DownloadFileAsync(int id);
    
    /// <summary>
    /// Get all thge latest and active downloads in a list
    /// </summary>
    /// <returns>List with DownloadResponse</returns>
    Task<Result<List<DownloadResponse>>> GetLatestAsync();
    
    /// <summary>
    /// Uploads a new download file
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="version">Version number</param>
    /// <param name="platform">Target platform</param>
    /// <returns>Result with upload response or failure</returns>
    Task<Result<DownloadResponse>> UploadFileAsync(IFormFile file, string version, DownloadPlatform platform);
}