using Lyn.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController(IDownloadService downloadService) : BaseController
{   
    /// <summary>
    /// Downloads a specific, active file for all platforms
    /// </summary>
    /// <param name="id">File Id</param>
    /// <returns>Ok 200 with a download file or 404 Not Found</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var result = await downloadService.DownloadFileAsync(id);
        
        if (result.IsFailure)
            return HandleFailure(result);

        var download = result.Value;
        return File(download!.FileData, download.ContentType, download.FileName);
    }
    
    /// <summary>
    /// Get all the latest and active files to download
    /// </summary>
    /// <returns>List with DownloadResponse or 500 Interal Server Error</returns>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestDownloads()
    {
        var result = await downloadService.GetLatestAsync();
        
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}