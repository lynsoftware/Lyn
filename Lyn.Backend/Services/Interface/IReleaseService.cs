using Lyn.Backend.DTOs;
using Lyn.Backend.DTOs.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;


public interface IReleaseService
{
    /// <summary>
    /// Laster opp en AppRelease-fil til S3 Bucket og lagrer filen til databasen
    /// </summary>
    /// <param name="request">UploadReleaseRequest med Versjon, Release Type, File og ReleaseNotes</param>
    /// <param name="ct">CT</param>
    /// <returns>Result med Success eller Failure</returns>
    Task<Result> UploadReleaseAsync(UploadReleaseRequest request, CancellationToken ct);

    /// <summary>
    /// Henter alle siste versjonene til hver Release Type
    /// </summary>
    /// <returns>Liste med AppReleaseResponse eller tom liste</returns>
    Task<Result<List<AppReleaseResponse>>> GetLatestAsync();
    
    /// <summary>
    /// Laster ned en AppRelease fra en S3 Bucket
    /// </summary>
    /// <param name="id">ID-en til AppRelease</param>
    /// <param name="ct"></param>
    /// <returns>FileDownloadDto </returns>
    Task<Result<FileDownloadDto>> DownloadAsync(int id, CancellationToken ct = default);
}