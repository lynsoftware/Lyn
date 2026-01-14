using Lyn.Backend.Models;
using Lyn.Backend.Models.Enums;
using Lyn.Backend.Repository;
using Lyn.Shared.Enum;
using Lyn.Shared.Models;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public class DownloadService(IDownloadRepository downloadRepository, 
    ILogger<DownloadService> logger, 
    IStatisticsRepository statisticsRepository) : IDownloadService
{
    // See interface for summary
    public async Task<Result<FileDownloadDto>> DownloadFileAsync(int id)
    {
        var file = await downloadRepository.DownloadFileAsync(id);

        if (file == null)
        {
            logger.LogWarning("File with Id {Id} does not exist or is not active", id);
            return Result<FileDownloadDto>.Failure("File does not exist", ErrorTypeEnum.NotFound);
        }
        
        var response = new FileDownloadDto
        {
            FileData = file.FileData,
            ContentType = file.ContentType,
            FileName = file.FileName
        };

        return Result<FileDownloadDto>.Success(response);
    }
    
    // See interface for summary
    public async Task<Result<List<DownloadResponse>>> GetLatestAsync()
    {
        var latestDownloads = await downloadRepository.GetLatestAsync();

        if (latestDownloads.Count == 0)
        {
            logger.LogWarning("No active files to download");
            return Result<List<DownloadResponse>>.Failure("No active files to download", 
                ErrorTypeEnum.InternalServerError);
        }

        var response = latestDownloads.Select(d => new DownloadResponse
        {
            Id = d.Id,
            FileName = d.FileName,
            Platform = d.Platform,
            Version = d.Version,
            UploadedAt = d.UploadedAt,
            FileSizeBytes = d.FileSizeBytes
        }).ToList();

        return Result<List<DownloadResponse>>.Success(response);
    }
    
    // See interface for summary
    public async Task<Result<DownloadResponse>> UploadFileAsync(
        IFormFile file, 
        string version, 
        DownloadPlatform platform)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var download = new AppDownload
            {
                FileName = file.FileName,
                Version = version,
                Platform = platform,
                FileData = fileBytes,
                FileSizeBytes = fileBytes.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                IsActive = true,
                FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant()
            };

            await downloadRepository.AddAsync(download);
            await downloadRepository.SaveChangesAsync();

            logger.LogInformation(
                "File uploaded successfully: {FileName} ({Platform}) - {Size} MB",
                file.FileName, platform, fileBytes.Length / 1024.0 / 1024.0);

            var response = new DownloadResponse
            {
                Id = download.Id,
                FileName = file.FileName,
                Version = version,
                Platform = platform,
                FileSizeBytes = fileBytes.Length,
                UploadedAt = download.UploadedAt
            };

            return Result<DownloadResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return Result<DownloadResponse>.Failure(
                "An error occurred while uploading the file", 
                ErrorTypeEnum.InternalServerError);
        }
    }
}