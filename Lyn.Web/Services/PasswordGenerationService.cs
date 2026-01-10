using System.Net.Http.Json;
using Lyn.Shared.Models;
using Lyn.Shared.Result;

namespace Lyn.Web.Services;

public class PasswordGenerationService(HttpClient httpClient, 
    ILogger<PasswordGenerationService> logger) : IPasswordGenerationService
{   
    // See interface for summary
    public async Task<Result<PasswordGenerationResponse>> GeneratePasswordAsync(PasswordGenerationRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/passwordgenerator", request, cancellationToken);

            response.EnsureSuccessStatusCode();
            
            var generatedPassword = await response.Content
                .ReadFromJsonAsync<PasswordGenerationResponse>(cancellationToken);
            
            if (generatedPassword == null)
            {
                logger.LogError("Failed to deserialize generated password");
                return Result<PasswordGenerationResponse>.Failure("Could not generate password");
            }
            
            return Result<PasswordGenerationResponse>.Success(generatedPassword);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when generating password. Request: {@Payload}", 
                request);
            return Result<PasswordGenerationResponse>.Failure("Could not generate password");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when generating password");
            return Result<PasswordGenerationResponse>.Failure("Unexpected error occurred");
        }
    }

}