using System.Net.Http.Headers;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Components.Forms;

namespace Lyn.Web.Services.Api;

public class SupportTicketService(
    ILogger<SupportTicketService> logger, 
    HttpClient httpClient) : ISupportTicketService
{
    /// <summary>
    /// Sends a users support ticket with optional file attachments to the backend
    /// </summary>
    /// <param name="ticketRequest">The support ticket request containing email, title, category, and description</param>
    /// <param name="attachments">Optional list of image file attachments (max 5 files, 5MB each)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result with Statuscode 204 No Content or error Message</returns>
    public async Task<Result> CreateSupportTicketAsync(
        SupportTicketRequest ticketRequest,
        List<IBrowserFile>? attachments, 
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating support ticket from {Email}, titled {Title}", 
                ticketRequest.Email, ticketRequest.Title);
            
            // Opprett multipart form data
            using var content = new MultipartFormDataContent();
            
            // Legg til request-feltene
            content.Add(new StringContent(ticketRequest.Email), nameof(SupportTicketRequest.Email));
            content.Add(new StringContent(ticketRequest.Title), nameof(SupportTicketRequest.Title));
            content.Add(new StringContent(ticketRequest.Category), nameof(SupportTicketRequest.Category));
            content.Add(new StringContent(ticketRequest.Description), nameof(SupportTicketRequest.Description));

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    var stream = file.OpenReadStream(
                        maxAllowedSize: SupportTicketFileConfig.TicketMaxFileSizeBytes);
                    
                    var fileContent = new StreamContent(stream);
                    
                    var contentType = string.IsNullOrWhiteSpace(file.ContentType) 
                        ? FileConstants.GetContentType(file.Name)
                        : file.ContentType;

                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    content.Add(fileContent, "attachments", file.Name);
                }
            }
            
            var response = await httpClient.PostAsync("api/support", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Upload failed: {Error}", errorContent);
                return Result.Failure(errorContent);
            }
            
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error during file upload");
            return Result.Failure("Connection failed. Please check your internet.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during file upload");
            return Result.Failure("Unexpected error occurred. Try again later.");
        }
    }
}