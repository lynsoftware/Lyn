using Lyn.Backend.Infrastructure.Persistence;
using Lyn.Shared.Models;

namespace Lyn.Backend.Platform.Support.Repositories;

public class SupportRepository(AppDbContext context) : ISupportRepository
{   
    // See interface for summary
    public async Task CreateSupportTicketAsync(SupportTicket ticket, CancellationToken ct = default)
    {
        await context.SupportTickets.AddAsync(ticket, ct);
        await context.SaveChangesAsync(ct);
    }
}