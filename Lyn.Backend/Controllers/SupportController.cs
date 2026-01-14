using Lyn.Backend.Services;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Request;
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
    /// <returns></returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FileSupportTicketUploadConstants.TicketMaxTotalRequestSize)] 
    [RequestFormLimits(MultipartBodyLengthLimit = FileSupportTicketUploadConstants.TicketMaxTotalRequestSize)]
    public async Task<IActionResult> CreateSupportTicket(
        [FromForm] SupportTicketRequest ticketRequest,
        [FromForm] List<IFormFile>? attachments)
    {
        var result = await supportTicketService.CreateSupportTicketAsync(ticketRequest, attachments);

        if (result.IsFailure)
            return HandleFailure(result);

        return NoContent();
    }
}