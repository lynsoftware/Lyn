using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Components.Forms;

namespace Lyn.Web.Services.Api;

public interface ISupportTicketService
{
    Task<Result> CreateSupportTicketAsync(
        SupportTicketRequest ticketRequest,
        List<IBrowserFile>? attachments,
        CancellationToken cancellationToken);
}