using Lyn.Backend.Services.Interface;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportController(ISupportTicketService supportTicketService) : BaseController
{
    /// <summary>
    /// Creates a new support ticket with optional file attachments.
    /// </summary>
    /// <param name="ticketRequest">The support ticket request containing email, title, category, and description.</param>
    /// <param name="attachments">Optional list of image file attachments (max 5 files, 5MB each).</param>
    /// <returns>Ok 200 med SupportTicketResponse</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(SupportTicketFileConfig.TicketMaxTotalRequestSize)] 
    [RequestFormLimits(MultipartBodyLengthLimit = SupportTicketFileConfig.TicketMaxTotalRequestSize)]
    [ProducesResponseType(typeof(SupportTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSupportTicket(
        [FromForm] SupportTicketRequest ticketRequest,
        [FromForm] List<IFormFile>? attachments)
    {
        var result = await supportTicketService.CreateSupportTicketAsync(ticketRequest, attachments);

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}