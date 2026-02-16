using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;

public interface ISupportTicketService
{
    /// <summary>
    /// Oppretter en Support Ticket med/uten Attachments. Validerer og laster opp filene til S3 hvis filer er vedlagt
    /// </summary>
    /// <param name="ticketRequest">SupportTicketRequest</param>
    /// <param name="attachments">Bilder som attachments</param>
    /// <param name="ct"></param>
    /// <returns>Result med SupportTicketResponse eller failure</returns>
    Task<Result<SupportTicketResponse>> CreateSupportTicketAsync(SupportTicketRequest ticketRequest,
        List<IFormFile>? attachments, CancellationToken ct = default);
}