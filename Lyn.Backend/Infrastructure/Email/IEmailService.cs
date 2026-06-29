using Lyn.Shared.Models;

namespace Lyn.Backend.Infrastructure.Email;

public interface IEmailService
{
    Task SupportTicketReceivedNotificationEmail(SupportTicket ticket);

    Task SendSupportTicketConfirmationAsync(SupportTicket ticket);
}