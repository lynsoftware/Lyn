using Lyn.Shared.Models;

namespace Lyn.Backend.Services.Interface;

public interface IEmailService
{
    Task SupportTicketReceivedNotificationEmail(SupportTicket ticket);

    Task SendSupportTicketConfirmationAsync(SupportTicket ticket);
}