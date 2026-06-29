using Lyn.Backend.Common.Controllers;
using Lyn.Backend.Platform.Auth.Services;
using Lyn.Shared.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Platform.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController(IAuthService authService) : BaseController
{   
    
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