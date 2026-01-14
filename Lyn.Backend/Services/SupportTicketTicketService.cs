using System.Security.Cryptography;
using Lyn.Backend.Repository;
using Lyn.Shared.Configuration;
using Lyn.Shared.Helpers;
using Lyn.Shared.Models;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;


namespace Lyn.Backend.Services;

public class SupportTicketTicketService(
    ILogger<SupportTicketTicketService> logger, 
    ISupportRepository supportRepository,
    IEmailService emailService
    ) : ISupportTicketService
{
    /// <summary>
    /// Creates a new support ticket with optional file attachments
    /// </summary>
    /// <param name="ticketRequest">The support ticket request containing email, title, category,
    /// and description</param>
    /// <param name="attachments">Optional list of image file attachments (max 5 files, 5MB each)</param>
    /// <returns>Result with success or failure with errorMessage</returns>
    public async Task<Result> CreateSupportTicketAsync(SupportTicketRequest ticketRequest, 
        List<IFormFile>? attachments)
    {
        var ticket = new SupportTicket
        {
            Email = ticketRequest.Email,
            Title = ticketRequest.Title,
            Category = ticketRequest.Category,
            Description = ticketRequest.Description
        };
        
        // Validate and save files
        if (attachments != null && attachments.Count > 0)
        {
            if (attachments.Count > FileSupportTicketUploadConstants.TicketMaxFileCount)
                return Result.Failure(
                    $"Maximum {FileSupportTicketUploadConstants.TicketMaxFileCount} files allowed");

            foreach (var file in attachments)
            {
                if (file.Length == 0)
                    return Result.Failure($"File '{file.FileName}' is empty");

                if (file.Length > FileSupportTicketUploadConstants.TicketMaxFileSizeBytes)
                    return Result.Failure(
                        $"File '{file.FileName}' exceeds maximum size of " +
                        $"{FileHelper.FormatFileSize(FileSupportTicketUploadConstants.TicketMaxFileSizeBytes)}");

                if (!FileHelper.IsValidImageExtension(
                        file.FileName, FileSupportTicketUploadConstants.AllowedImageExtensions))
                {
                    return Result.Failure(
                        $"File '{file.FileName}' has invalid extension. Allowed: " +
                        $"{string.Join(", ", FileSupportTicketUploadConstants.AllowedImageExtensions)}");
                }
                
                if (!FileHelper.IsValidImageType(
                        file.ContentType, FileSupportTicketUploadConstants.AllowedImageTypes))
                {
                    return Result.Failure(
                        $"File '{file.FileName}' has invalid content type. Allowed: " +
                        $"{string.Join(", ", FileSupportTicketUploadConstants.AllowedImageTypes)}"); 
                }
                
                var attachment = await CreateAttachmentAsync(file);
                ticket.Attachments.Add(attachment);
            }
        }
        
        await supportRepository.CreateSupportTicketAsync(ticket);
        
        logger.LogInformation(
            "Support ticket created: {TicketId} from {Email} with {AttachmentCount} attachments",
            ticket.Id, ticket.Email, ticket.Attachments.Count);
        
        try
        {
            await emailService.SendSupportTicketConfirmationAsync(ticket);
            await emailService.SupportTicketReceivedNotificationEmail(ticket);
            logger.LogInformation("Confirmation email sent for ticket {TicketId}", ticket.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send confirmation email for ticket {TicketId}", ticket.Id);
        }

        return Result.Success();
    }
    
    
    /// <summary>
    /// Creates a SupportAttachment object from an uploaded file with hash calculation.
    /// </summary>
    /// <param name="file">The uploaded file to process.</param>
    /// <returns>A SupportAttachment with file data, hash, and metadata.</returns>
    private async Task<SupportAttachment> CreateAttachmentAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        // Estimate and get hash
        memoryStream.Position = 0;
        string fileHash;
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = await sha256.ComputeHashAsync(memoryStream);
            fileHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
        
        // Get extension and the new fileName
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var newFileName = $"{Guid.NewGuid()}{extension}";
        
        return new SupportAttachment
        {
            FileName = newFileName,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileExtension = extension,
            FileSize = file.Length,
            FilePath = $"support-attachments/{newFileName}",
            FileHash = fileHash,
            FileData = fileBytes,
            UploadedAt = DateTime.UtcNow
        };
    }
}