using System.Net.Http.Json;
using Blazored.SessionStorage;
using Lyn.Shared.Enum;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;
using Lyn.Web.Common.Extensions;

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
            logger.LogInformation("Login attempt from Email: {@Payload}", new { email = request.Email });

            var response = await httpClient.PostAsJsonAsync("api/admin/login", request, cancellationToken);

            // Token kommer som en JSON-streng; ParseResponseAsync henter ut code+detail ved feil
            var tokenResult = await HttpClientExtensions.ParseResponseAsync<string>(response, cancellationToken);
            if (tokenResult.IsFailure)
            {
                logger.LogWarning("Login failed: {Error} ({Code})", tokenResult.Error, tokenResult.ErrorCode);
                return Result.Failure(tokenResult.Error, tokenResult.ErrorCode);
            }

            var token = tokenResult.Value!;
            if (string.IsNullOrEmpty(token))
                return Result.Failure("Login failed", AppErrorCode.Unknown);

            await sessionStorage.SetItemAsStringAsync(TokenKey, token, cancellationToken);
            logger.LogInformation("Login successful for email: {Email}", request.Email);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error during login attempt");
            return Result.Failure("Connection failed. Please check your internet.", AppErrorCode.Unknown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred");
            return Result.Failure("Unexpected error occurred. Try again later.", AppErrorCode.Unknown);
        }
    }
}