using Lyn.Backend.EmailTemplates;
using Lyn.Backend.Services.Interface;
using Lyn.Shared.Models;
using Resend;

namespace Lyn.Backend.Services;

public class EmailService(IResend resend) : IEmailService
{
    /// <summary>
    /// Verifies Support Ticket is created to the creator email
    /// </summary>
    public async Task SendSupportTicketConfirmationAsync(SupportTicket ticket)
    {
        var message = new EmailMessage
        {
            From = "support@lynsoftware.com", 
            To = ticket.Email,
            Subject = $"Support Ticket #{ticket.Id} Received - Lyn Software",
            HtmlBody = SupportTicketTemplates.SupportTicketConfirmation(ticket)
        };

        var response = await resend.EmailSendAsync(message);
        
        if (!response.Success)
        {
            throw new InvalidOperationException(
                $"Failed to send confirmation email for ticket {ticket.Id}. " +
                $"Error: {response.Exception?.Message ?? "Unknown error"}");
        }
    }
    
    /// <summary>
    /// Verifies support@lynsoftware.com with new support ticket created
    /// </summary>
    public async Task SupportTicketReceivedNotificationEmail(SupportTicket ticket)
    {
        var message = new EmailMessage
        {
            From = "donotreply@lynsoftware.com",
            To = "support@lynsoftware.com",
            Subject = $"Support Ticket #{ticket.Id} Received",
            HtmlBody = SupportTicketTemplates.SupportTicketNotification(ticket)
        };

        var response = await resend.EmailSendAsync(message);
        
        if (!response.Success)
        {
            throw new InvalidOperationException(
                $"Failed to send notification email for ticket {ticket.Id}. " +
                $"Error: {response.Exception?.Message ?? "Unknown error"}");
        }
    }
}