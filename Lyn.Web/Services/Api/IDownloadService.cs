using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;
using Lyn.Web.DTOs;

namespace Lyn.Web.Services.Api;

public interface IDownloadService
{
    /// <summary>
    /// Downloads a specific file by ID
    /// </summary>
    /// <param name="id">The ID of the file to download</param>
    /// <param name="cancellationToken"></param>
    /// <returns>File stream for download</returns>
    Task<Result<FileDownloadDto>> GetDownloadAsync(int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest active download for each platform
    /// </summary>
    /// <returns>List of latest downloads with metadata</returns>
    Task<Result<List<AppReleaseResponse>>> GetLatestDownloadsAsync(
        CancellationToken cancellationToken = default);
}