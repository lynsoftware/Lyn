using Lyn.Shared.Enum;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;
using Lyn.Web.Common.Extensions;
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

            // Nedlasting er binær, så vi kan ikke bruke ParseResponseAsync på suksess-stien.
            // Ved feil henter vi likevel ut code + detail via extensionen.
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to download file with id {Id}. Status: {Status}",
                    id, response.StatusCode);
                var problem = await HttpClientExtensions.ParseEmptyResponseAsync(response, cancellationToken);
                return Result<FileDownloadDto>.Failure(problem.Error, problem.ErrorCode);
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
            return Result<FileDownloadDto>.Failure("Could not download file", AppErrorCode.Unknown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when downloading file {Id}", id);
            return Result<FileDownloadDto>.Failure("Unexpected error occurred", AppErrorCode.Unknown);
        }
    }

    // See interface for summary
    public async Task<Result<List<AppReleaseResponse>>> GetLatestDownloadsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("api/AppRelease/latest", cancellationToken);
            return await HttpClientExtensions.ParseResponseAsync<List<AppReleaseResponse>>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when getting latest downloads");
            return Result<List<AppReleaseResponse>>.Failure("Could not retrieve downloads", AppErrorCode.Unknown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when getting latest downloads");
            return Result<List<AppReleaseResponse>>.Failure("Unexpected error occurred", AppErrorCode.Unknown);
        }
    }
}
