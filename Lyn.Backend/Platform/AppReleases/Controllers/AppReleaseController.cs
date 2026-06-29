using Lyn.Backend.Common.Controllers;
using Lyn.Backend.Platform.AppReleases.DTOs.Requests;
using Lyn.Backend.Platform.AppReleases.Security.Filters;
using Lyn.Backend.Platform.AppReleases.Services;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Platform.AppReleases.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppReleaseController(IAppReleaseService appReleaseService) : BaseController
{
    /// <summary>
    /// Laster opp en release til databasen og S3 Bucket. Brukes fra Github
    /// </summary>
    /// <param name="request">UploadReleaseRequest med Versjon, Release Type, File og ReleaseNotes</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    [ApiKeyAuth] // Nøkkel som blir sjekket med filter
    [RequestSizeLimit(AppReleaseFileConfig.AppReleaseMaxSizeInBytes)] // Maks størrelse på hele forespørselen
    [RequestFormLimits(MultipartBodyLengthLimit = AppReleaseFileConfig.AppReleaseMaxSizeInBytes)]// Maks på Form Data
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // Hvis release allerede eksisterer
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadRelease([FromForm] UploadReleaseRequest request, CancellationToken ct)
    {
        var result = await appReleaseService.UploadReleaseAsync(request, ct);
        
        if (result.IsFailure)
            return HandleFailure(result);
      
        return Created();
    }
    
    
    /// <summary>
    /// Henter alle siste AppReleaser til frontend
    /// </summary>
    /// <returns>Liste med AppReleaseResponse</returns>
    [HttpGet("latest")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<AppReleaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestDownloads()
    {
        var result = await appReleaseService.GetLatestAsync();
        
        if (result.IsFailure)
            return HandleFailure(result);
        
        
        return Ok(result.Value);
    }
    
    /// <summary>
    /// Laster ned en spesifikk versjon av en release
    /// </summary>
    [HttpGet("download/{id}")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadReleaseVersion(int id, CancellationToken ct)
    {
        var result = await appReleaseService.DownloadAsync(id, ct);
        
        if (result.IsFailure)
            return HandleFailure(result);

        var file = result.Value;
        
        return File(
            file!.Stream, 
            file.ContentType, 
            file.FileName);
    }
}