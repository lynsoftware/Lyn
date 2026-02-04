using Lyn.Shared.Models;

namespace Lyn.Backend.Repository;

public interface ISupportRepository
{
    /// <summary>
    /// Adds and saves a Support ticket
    /// </summary>
    /// <param name="ticket">Support ticket sendt by user</param>
    /// <param name="ct"></param>
    Task CreateSupportTicketAsync(SupportTicket ticket, CancellationToken ct = default);
}