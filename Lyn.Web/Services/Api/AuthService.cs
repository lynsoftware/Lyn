using System.Net.Http.Json;
using Blazored.SessionStorage;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;

namespace Lyn.Web.Services.Api;

public class AuthService(ILogger<AuthService> logger, HttpClient httpClient, 
    ISessionStorageService sessionStorage) 
    : IAuthService
{
    
    private const string TokenKey = "adminToken";
    
    public async Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Login attempt from Email: {@Payload}", new { email = request.Email});

            var response = await httpClient.PostAsJsonAsync("api/admin/login", 
                request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Login failed: {Error}", errorContent);
                return Result.Failure(errorContent);
            }

            var token = await response.Content.ReadAsStringAsync(cancellationToken);
            
            token = token.Trim('"');
            
            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Login failed: Empty token received");
                return Result.Failure("Login failed");
            }

            // Lagre token i session storage
            await sessionStorage.SetItemAsStringAsync(TokenKey, token, cancellationToken);
            
            logger.LogInformation("Login successful for email: {Email}", request.Email);
        
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error during login attempt");
            return Result.Failure("Connection failed. Please check your internet.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred");
            return Result.Failure("Unexpected error occurred. Try again later.");
        }
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await sessionStorage.GetItemAsStringAsync(TokenKey, cancellationToken);
        return token?.Trim('"');
    }
    
}