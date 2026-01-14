using Lyn.Backend.Models.Enums;
using Lyn.Backend.Services;
using Lyn.Shared.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController(IDownloadService downloadService,
    IAuthService authService) : BaseController
{   
    /// <summary>
    /// Uploads a file to the database from a file sent with Postman. Requires Authetntication
    /// </summary>
    /// <param name="file">File as IFormFile</param>
    /// <param name="version">Version</param>
    /// <param name="platform">Platform</param>
    /// <returns>200 Ok or 400 Bad Request</returns>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(104_857_600)] // 100 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string version,
        [FromForm] DownloadPlatform platform)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        if (string.IsNullOrWhiteSpace(version))
            return BadRequest(new { error = "Version is required" });

        var result = await downloadService.UploadFileAsync(file, version, platform);

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
    
    /// <summary>
    /// Login
    /// </summary>
    /// <param name="request">LoginRequest with Email and Password</param>
    /// <returns>LoginResponseDto</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        
        if (result.IsFailure)
            return HandleFailure(result);
      
        return Ok(result.Value);
    }

}