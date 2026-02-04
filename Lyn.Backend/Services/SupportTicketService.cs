using Lyn.Backend.Repository;
using Lyn.Backend.Services.Interface;
using Lyn.Backend.Validators;
using Lyn.Shared.Configuration;
using Lyn.Shared.Enum;
using Lyn.Shared.Models;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;


namespace Lyn.Backend.Services;

public class SupportTicketService(
    ILogger<SupportTicketService> logger, 
    ISupportRepository supportRepository,
    IEmailService emailService, 
    IFileValidator fileValidator,
    IStorageService storageService) : ISupportTicketService
{
    
    /// <summary>
    /// Oppretter en Support Ticket med/uten Attachments. Validerer og laster opp filene til S3 hvis filer er vedlagt
    /// </summary>
    /// <param name="ticketRequest">SupportTicketRequest</param>
    /// <param name="attachments">Bilder som attachments</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<SupportTicketResponse>> CreateSupportTicketAsync(SupportTicketRequest ticketRequest, 
        List<IFormFile>? attachments, CancellationToken ct = default)
    {
        // Oppretter en SupportTicket
        var ticket = new SupportTicket
        {
            Email = ticketRequest.Email,
            Title = ticketRequest.Title,
            Category = ticketRequest.Category,
            Description = ticketRequest.Description
        };
        
        // Validerer og laster opp filer til S3
        if (attachments != null && attachments.Count > 0)
        {
            // Sjekk maks antall filer
            if (attachments.Count > SupportTicketFileConfig.TicketMaxFileCount)
                return Result<SupportTicketResponse>.Failure(
                    $"Maximum {SupportTicketFileConfig.TicketMaxFileCount} files allowed");
            
            // Validerer og laster opp filer
            var attachmentResult = await ValidateAndUploadAttachmentsAsync(attachments, ct);
            if (attachmentResult.IsFailure)
                return Result<SupportTicketResponse>.Failure(attachmentResult.Error);
            
            ticket.Attachments = attachmentResult.Value!;
        }
        
        // Lagre i database
        try
        {
            await supportRepository.CreateSupportTicketAsync(ticket, ct);
            
            logger.LogInformation(
                "Support ticket created: {TicketId} from {Email} with {AttachmentCount} attachments",
                ticket.Id, ticket.Email, ticket.Attachments.Count);
        }
        catch (Exception ex)
        {
            // Database feilet - slett opplastede filer fra S3
            logger.LogError(ex, "Failed to save support ticket. Cleaning up uploaded files.");
            await CleanupUploadedFilesAsync(ticket.Attachments, ct);
            
            return Result<SupportTicketResponse>.Failure("Failed to create support ticket. Please try again.");
        }
        
        // Send e-poster (ikke kritisk - feiler stille)
        await SendEmailNotificationsAsync(ticket);
        
        return Result<SupportTicketResponse>.Success(new SupportTicketResponse
        {
            SupportTicketId = ticket.Id,
            NumberOfAttachments = attachments?.Count ?? 0
        });
    }
    
    /// <summary>
    /// Iterer igjennom hver fil, validerer og laster opp til S3 Bucket. Hvis en fil feiler så utfører vi
    /// CleanupUploadedFilesAsync som sletter alle filene
    /// </summary>
    /// <param name="attachments">Alle attachmentene vi skal validere og laste opp</param>
    /// <param name="ct"></param>
    /// <returns>Result med en liste med SupportAttachments eller Failure med Error</returns>
    private async Task<Result<List<SupportAttachment>>> ValidateAndUploadAttachmentsAsync(
        List<IFormFile> attachments,
        CancellationToken ct)
    {
        // En liste vi legger attachmentene til etter vellykket lagring
        var uploadedAttachments = new List<SupportAttachment>();
        
        // Iterer igjennom hver fil
        foreach (var file in attachments)
        {
            // Validerer og laster opp filen til S3
            var result = await ValidateAndUploadSingleAsync(file, ct);
            
            // Feiler en så sletter vi de andre filene fra S3
            if (result.IsFailure)
            {   
                await CleanupUploadedFilesAsync(uploadedAttachments, ct);
                return Result<List<SupportAttachment>>.Failure(result.Error);
            }
        
            uploadedAttachments.Add(result.Value!);
        }
    
        return Result<List<SupportAttachment>>.Success(uploadedAttachments);
    }

    /// <summary>
    /// Validerer en enkel fil og laster den opp til S3
    /// </summary>
    /// <param name="file"></param>
    /// <param name="ct"></param>
    /// <returns>Result med SupportAttachmeet hvis vellykket eller Failure</returns>
    private async Task<Result<SupportAttachment>> ValidateAndUploadSingleAsync(IFormFile file, CancellationToken ct)
    {
        // Validerer filen
        var validationResult = fileValidator.ValidateSupportAttachment(file);
        if (validationResult.IsFailure)
            return Result<SupportAttachment>.Failure(validationResult.Error, ErrorTypeEnum.Validation);
    
        // Lastrer filen opp til S3
        var uploadResult = await UploadAttachmentAsync(file, ct);
        if (uploadResult.IsFailure)
            return Result<SupportAttachment>.Failure(uploadResult.Error);

        return Result<SupportAttachment>.Success(uploadResult.Value!);
    }
    
    /// <summary>
    /// Laster opp en fil til en S3 bucket og oppretter et SupportAttachment klar til algring
    /// </summary>
    /// <param name="file">Filen som skal lastes opp</param>
    /// <param name="ct"></param>
    /// <returns>Result med filen SupportAttachment eller Failure</returns>
    private async Task<Result<SupportAttachment>> UploadAttachmentAsync(IFormFile file, CancellationToken ct)
    {
        // Henter extension, oppretter en Guid og lager en storageKey for filen   
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileId = Guid.NewGuid();
        var storageKey = $"support-attachments/{fileId}{extension}";
        
        // Åpner en stream av filen og laster opp til S3
        await using var stream = file.OpenReadStream();
        var uploadResult = await storageService.UploadAsync(stream, storageKey, file.ContentType, ct);
        if (uploadResult.IsFailure)
        {
            logger.LogError("Failed to upload attachment to S3: {FileName}", file.FileName);
            return Result<SupportAttachment>.Failure("Failed to upload attachment");
        }
        
        // Returnerer det som en SupportAttachment
        return Result<SupportAttachment>.Success(new SupportAttachment
        {
            FileId = fileId,
            FileName = $"{fileId}{extension}",
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileExtension = extension,
            FileSize = file.Length,
            StorageKey = storageKey,
            UploadedAt = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Hvis noe går galt under opplastning så sletter vi alle filene igjen
    /// </summary>
    /// <param name="attachments">Filene som har blitt lastet opp og må slettes</param>
    /// <param name="ct"></param>
    private async Task CleanupUploadedFilesAsync(List<SupportAttachment> attachments, CancellationToken ct)
    {
        foreach (var attachment in attachments)
        {
            var deleteResult = await storageService.DeleteAsync(attachment.StorageKey, ct);
            if (deleteResult.IsFailure)
                logger.LogError("Failed to cleanup file: {StorageKey}", attachment.StorageKey);
        }
    }
    
    /// <summary>
    /// Sender e-postvarsler
    /// </summary>
    private async Task SendEmailNotificationsAsync(SupportTicket ticket)
    {
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
    }
}