using Lyn.Shared.Models;

namespace Lyn.Backend.EmailTemplates;

public class SupportTicketTemplates
{
    private const string BaseUrl = "https://lynsoftware.com"; 
    
    /// <summary>
    /// Email template for Support Ticket Confirmation
    /// </summary>
    public static string SupportTicketConfirmation(SupportTicket ticket)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; background-color: #1a1a1a; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                <table role='presentation' style='width: 600px; max-width: 90%; border-collapse: collapse; background-color: #2d2d2d; border-radius: 12px; overflow: hidden;'>
                    
                    <!-- Header with Logo -->
                    <tr>
                        <td style='padding: 40px 40px 20px; text-align: center; background-color: #1a1a1a;'>
                            <img src='{BaseUrl}/images/passwordgeneratorlogo.png' alt='Lyn Software' style='height: 60px; margin-bottom: 20px;'>
                            <h1 style='margin: 0; color: #FEE447; font-size: 28px; font-weight: 600;'>Support Ticket Received</h1>
                        </td>
                    </tr>
                    
                    <!-- Body -->
                    <tr>
                        <td style='padding: 40px;'>
                            <p style='margin: 0 0 20px; color: #b0b0b0; font-size: 16px; line-height: 1.6;'>
                                Thank you for contacting Lyn Software. We have received your support ticket and will respond as soon as possible.
                            </p>
                            
                            <!-- Ticket Details Box -->
                            <table role='presentation' style='width: 100%; border-collapse: collapse; background-color: #1a1a1a; border-radius: 8px; margin: 30px 0;'>
                                <tr>
                                    <td style='padding: 24px;'>
                                        <h2 style='margin: 0 0 16px; color: #FEE447; font-size: 18px; font-weight: 600;'>Ticket Details</h2>
                                        
                                        <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                                            <tr>
                                                <td style='padding: 8px 0; color: #808080; font-size: 14px; width: 120px;'>Ticket ID:</td>
                                                <td style='padding: 8px 0; color: #FEE447; font-size: 14px; font-weight: 600;'>#{ticket.Id}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #808080; font-size: 14px;'>Category:</td>
                                                <td style='padding: 8px 0; color: #b0b0b0; font-size: 14px;'>{ticket.Category}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #808080; font-size: 14px;'>Title:</td>
                                                <td style='padding: 8px 0; color: #b0b0b0; font-size: 14px;'>{ticket.Title}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #808080; font-size: 14px;'>Created:</td>
                                                <td style='padding: 8px 0; color: #b0b0b0; font-size: 14px;'>{ticket.CreatedAt:yyyy-MM-dd HH:mm}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style='margin: 20px 0; color: #b0b0b0; font-size: 16px; line-height: 1.6;'>
                                We typically respond within <strong style='color: #FEE447;'>24-48 hours</strong>.
                            </p>
                            
                            <p style='margin: 30px 0 0; color: #b0b0b0; font-size: 16px; line-height: 1.6;'>
                                Best regards,<br>
                                <strong style='color: #FEE447;'>Lyn Software Support Team</strong>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 30px 40px; background-color: #1a1a1a; text-align: center; border-top: 1px solid #404040;'>
                            <p style='margin: 0; color: #808080; font-size: 14px;'>
                                © {DateTime.Now.Year} Lyn Software. All rights reserved.
                            </p>
                            <p style='margin: 10px 0 0; color: #808080; font-size: 12px;'>
                                This is an automated message, please do not reply directly to this email.
                            </p>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
    
    /// <summary>
    /// Email template notifiying the support team of new ticket
    /// </summary>
    public static string SupportTicketNotification(SupportTicket ticket)
    {
        return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background-color: #f8f9fa; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
        <table role='presentation' style='width: 100%; border-collapse: collapse;'>
            <tr>
                <td align='center' style='padding: 40px 20px;'>
                    <table role='presentation' style='width: 600px; max-width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                        
                        <!-- Header -->
                        <tr>
                            <td style='padding: 30px 30px 20px; background-color: #FEE447; border-radius: 8px 8px 0 0;'>
                                <h1 style='margin: 0; color: #1F1F1F; font-size: 24px; font-weight: 600;'>New Support Ticket</h1>
                            </td>
                        </tr>
                        
                        <!-- Body -->
                        <tr>
                            <td style='padding: 30px;'>
                                
                                <!-- Ticket Info Table -->
                                <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='color: #6c757d; font-size: 14px; font-weight: 600;'>Ticket ID:</span>
                                        </td>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0; text-align: right;'>
                                            <span style='color: #2C3E50; font-size: 14px; font-weight: 700;'>#{ticket.Id}</span>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='color: #6c757d; font-size: 14px; font-weight: 600;'>From:</span>
                                        </td>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0; text-align: right;'>
                                            <span style='color: #2C3E50; font-size: 14px;'>{ticket.Email}</span>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='color: #6c757d; font-size: 14px; font-weight: 600;'>Category:</span>
                                        </td>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0; text-align: right;'>
                                            <span style='color: #2C3E50; font-size: 14px;'>{ticket.Category}</span>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='color: #6c757d; font-size: 14px; font-weight: 600;'>Created:</span>
                                        </td>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0; text-align: right;'>
                                            <span style='color: #2C3E50; font-size: 14px;'>{ticket.CreatedAt:yyyy-MM-dd HH:mm} UTC</span>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0;'>
                                            <span style='color: #6c757d; font-size: 14px; font-weight: 600;'>Attachments:</span>
                                        </td>
                                        <td style='padding: 12px 0; border-bottom: 1px solid #e0e0e0; text-align: right;'>
                                            <span style='color: #2C3E50; font-size: 14px;'>{ticket.Attachments.Count}</span>
                                        </td>
                                    </tr>
                                </table>
                                
                                <!-- Title -->
                                <div style='margin-top: 30px;'>
                                    <h3 style='margin: 0 0 10px; color: #2C3E50; font-size: 16px; font-weight: 600;'>Title:</h3>
                                    <p style='margin: 0; color: #2C3E50; font-size: 15px; line-height: 1.5;'>{ticket.Title}</p>
                                </div>
                                
                                <!-- Description -->
                                <div style='margin-top: 25px;'>
                                    <h3 style='margin: 0 0 10px; color: #2C3E50; font-size: 16px; font-weight: 600;'>Description:</h3>
                                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; border-left: 4px solid #FEE447;'>
                                        <p style='margin: 0; color: #2C3E50; font-size: 14px; line-height: 1.6; white-space: pre-wrap;'>{ticket.Description}</p>
                                    </div>
                                </div>
                                
                            </td>
                        </tr>
                        
                        <!-- Footer -->
                        <tr>
                            <td style='padding: 20px 30px; background-color: #f8f9fa; border-radius: 0 0 8px 8px; text-align: center;'>
                                <p style='margin: 0; color: #6c757d; font-size: 13px;'>
                                    Lyn Software Support System
                                </p>
                            </td>
                        </tr>
                        
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";
    }
    
    
}