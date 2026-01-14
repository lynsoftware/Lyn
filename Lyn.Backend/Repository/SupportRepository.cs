using Lyn.Backend.Data;
using Lyn.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Repository;

public class SupportRepository(AppDbContext context) : ISupportRepository
{   
    // See interface for summary
    public async Task CreateSupportTicketAsync(SupportTicket ticket)
    {
        await context.SupportTickets.AddAsync(ticket);
        await context.SaveChangesAsync();
    }
}