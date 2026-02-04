using System.Net.Http.Json;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;
using Lyn.Web.DTOs;

namespace Lyn.Web.Services.Api;

public class DownloadService(HttpClient httpClient, 
    ILogger<PasswordGenerationService> logger) : IDownloadService
{
    // See interface for summary
    public async Task<Result<FileDownloadDto>> GetDownloadAsync(int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/AppRelease/download/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to download file with id {Id}. Status: {Status}", 
                    id, response.StatusCode);
                return Result<FileDownloadDto>.Failure("Could not download file");
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') 
                           ?? $"download_{id}";
            var contentType = response.Content.Headers.ContentType?.MediaType 
                              ?? "application/octet-stream";
            
            var fileDownload = new FileDownloadDto
            {
                FileData = fileBytes,
                ContentType = contentType,
                FileName = fileName
            };
            
            return Result<FileDownloadDto>.Success(fileDownload);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when downloading file {Id}", id);
            return Result<FileDownloadDto>.Failure("Could not download file");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when downloading file {Id}", id);
            return Result<FileDownloadDto>.Failure("Unexpected error occurred");
        }
    }
    
    // See interface for summary
    public async Task<Result<List<AppReleaseResponse>>> GetLatestDownloadsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var downloads = await httpClient.GetFromJsonAsync<List<AppReleaseResponse>>(
                "api/AppRelease/latest", cancellationToken);

            if (downloads == null)
            {
                logger.LogError("Failed to deserialize download list");
                return Result<List<AppReleaseResponse>>.Failure("Could not retrieve downloads. Try again later");
            }
            
            return Result<List<AppReleaseResponse>>.Success(downloads);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when getting latest downloads");
            return Result<List<AppReleaseResponse>>.Failure("Could not retrieve downloads");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when getting latest downloads");
            return Result<List<AppReleaseResponse>>.Failure("Unexpected error occurred");
        }
    }
}