using Lyn.Backend.DTOs.Request;
using Lyn.Backend.Filters;
using Lyn.Backend.Services.Interface;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppReleaseController(IReleaseService releaseService) : BaseController
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
        var result = await releaseService.UploadReleaseAsync(request, ct);
        
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
        var result = await releaseService.GetLatestAsync();
        
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
        var result = await releaseService.DownloadAsync(id, ct);
        
        if (result.IsFailure)
            return HandleFailure(result);

        var file = result.Value;
        
        return File(
            file!.Stream, 
            file.ContentType, 
            file.FileName);
    }
}