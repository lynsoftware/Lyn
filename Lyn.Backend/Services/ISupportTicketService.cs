using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public interface ISupportTicketService
{
    Task<Result> CreateSupportTicketAsync(SupportTicketRequest ticketRequest, List<IFormFile>? attachments);
}