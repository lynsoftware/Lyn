
using Lyn.Backend.Services.Interface;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordGeneratorController(IPasswordGeneratorService passwordGeneratorService) : BaseController
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
        var result = await passwordGeneratorService.GeneratePasswordAsync(request);
        
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}