using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;

public interface ISupportTicketService
{
    Task<Result<SupportTicketResponse>> CreateSupportTicketAsync(SupportTicketRequest ticketRequest,
        List<IFormFile>? attachments, CancellationToken ct = default);
}