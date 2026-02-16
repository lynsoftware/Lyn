using Lyn.Backend.DTOs;
using Lyn.Backend.DTOs.Request;
using Lyn.Backend.Models;
using Lyn.Backend.Repository;
using Lyn.Backend.Services.Interface;
using Lyn.Backend.Validators;
using Lyn.Shared.Enum;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public class ReleaseService(
    ILogger<ReleaseService> logger, 
    IFileValidator fileValidator, 
    IStorageService s3StorageService,
    IReleaseRepository releaseRepository) : IReleaseService
{
    /// <inheritdoc />
    public async Task<Result> UploadReleaseAsync(UploadReleaseRequest request, CancellationToken ct)
    {
        // Sjekk om release allerede eksisterer
        var exists = await releaseRepository.ExistsAsync(request.Version, request.Type, ct);
        if (exists)
        {
            logger.LogWarning("Release already exists: {Version} {Type}", request.Version, request.Type);
            return Result.Failure($"Release {request.Version} for {request.Type} already exists");
        }
        
        // Validerer filstørrelse, extension, content og magic type
        var validateFileResult = fileValidator.ValidateReleaseFile(request.File, request.Type);
        if (validateFileResult.IsFailure)
            return Result.Failure(validateFileResult.Error);
        
        // Hent data og oppretter storageKey for lagring av filen
        var file = request.File;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileId = Guid.NewGuid();
        var storageKey = $"releases/{request.Type}/{request.Version}/{fileId}{extension}";
        
        // Laster opp til storage
        await using var stream = file.OpenReadStream();
        var uploadResult = await s3StorageService.UploadAsync(stream, storageKey, file.ContentType, ct);
        
        if (uploadResult.IsFailure)
            return Result.Failure(uploadResult.Error);
        
        // Prøver å lagrer i databasen og hvis det feiler så slettes filen fra bøtta
        try
        {
            var release = new AppRelease
            {
                FileName = file.FileName,
                Version = request.Version,
                Type = request.Type,
                FileGuidId = fileId,
                StorageKey = storageKey,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                ReleaseNotes = request.ReleaseNotes,
                FileExtension = extension
            };

            await releaseRepository.CreateAsync(release, ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save release to database. " +
                                "Attempting to delete orphaned file from S3: {StorageKey}", storageKey);

            // Forsøk å slette fra S3
            var deleteResult = await s3StorageService.DeleteAsync(storageKey, ct);

            if (deleteResult.IsFailure)
                logger.LogError(
                    "Failed to delete orphaned file from S3: {StorageKey}. Manual cleanup required.", storageKey);
            
            return Result.Failure("Failed to save release. Please try again.");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<List<AppReleaseResponse>>> GetLatestAsync()
    {
        var latestDownloads = await releaseRepository.GetLatestAsync();

        if (latestDownloads.Count == 0)
        {
            logger.LogWarning("No active files to download");
            return Result<List<AppReleaseResponse>>.Failure("No active files to download", 
                ErrorTypeEnum.InternalServerError);
        }

        var response = latestDownloads.Select(d => new AppReleaseResponse
        {
            Id = d.Id,
            FileName = d.FileName,
            Type = d.Type,
            Version = d.Version,
            UploadedAt = d.UploadedAt,
            FileSizeBytes = d.FileSizeBytes
        }).ToList();

        return Result<List<AppReleaseResponse>>.Success(response);
    }
    
    /// <inheritdoc />
    public async Task<Result<FileDownloadDto>> DownloadAsync(int id, CancellationToken ct = default)
    {
        // Hent release fra database
        var appRelease = await releaseRepository.GetAsync(id, ct);
        
        if (appRelease is null)
        {
            logger.LogWarning("Release not found for id {AppReleaseId}", id);
            return Result<FileDownloadDto>.Failure($"Release not found", ErrorTypeEnum.NotFound);
        }
        
        // Last ned fra S3
        var downloadResult = await s3StorageService.DownloadAsync(appRelease.StorageKey, ct);
        
        if (downloadResult.IsFailure)
        {
            logger.LogError("Failed to download file from S3: {StorageKey}", appRelease.StorageKey);
            return Result<FileDownloadDto>.Failure(downloadResult.Error);
        }
        
        // Oppdater download count (fire and forget)
        _ = releaseRepository.IncrementDownloadCountAsync(appRelease.Id, ct);
        
        return Result<FileDownloadDto>.Success(new FileDownloadDto
        {
            Stream = downloadResult.Value!,
            ContentType = appRelease.ContentType,
            FileName = $"{appRelease.Type}-{appRelease.Version}{appRelease.FileExtension}"
        });
    }
}