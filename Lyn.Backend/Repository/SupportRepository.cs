using Lyn.Backend.Data;
using Lyn.Shared.Models;

namespace Lyn.Backend.Repository;

public class SupportRepository(AppDbContext context) : ISupportRepository
{   
    // See interface for summary
    public async Task CreateSupportTicketAsync(SupportTicket ticket, CancellationToken ct = default)
    {
        await context.SupportTickets.AddAsync(ticket, ct);
        await context.SaveChangesAsync(ct);
    }
}