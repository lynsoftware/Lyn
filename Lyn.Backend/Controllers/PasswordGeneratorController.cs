using Lyn.Backend.Services;
using Lyn.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordGeneratorController(IPasswordService passwordService) : BaseController
{   
    /// <summary>
    /// Endepunkt som genererer et passord for brukeren
    /// </summary>
    /// <param name="request">PasswordGenerationRequest</param>
    /// <returns>Ok med det genererte passordet eller </returns>
    [HttpPost]
    [ProducesResponseType(typeof(PasswordGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePassword(
        [FromBody] PasswordGenerationRequest request)
    {
        var result = await passwordService.GeneratePasswordAsync(request);
        
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}