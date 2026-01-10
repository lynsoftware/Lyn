using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using Lyn.Backend.Models.Enums;
using Lyn.Shared.Models;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Components.Forms;

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
            
            token = token?.Trim('"');
            
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
    
    public async Task<Result<string>> UploadFileAsync(
        IBrowserFile file, 
        string version, 
        DownloadPlatform platform, 
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("=== UPLOAD START ===");
            logger.LogInformation("File: {FileName}, Size: {Size} bytes", file.Name, file.Size);
            logger.LogInformation("Version: {Version}, Platform: {Platform}", version, platform);
            logger.LogInformation("ContentType: {ContentType}", file.ContentType);

            // Hent token
            var token = await GetTokenAsync(cancellationToken);
            logger.LogInformation("Token retrieved: {HasToken}, Length: {Length}", 
                !string.IsNullOrEmpty(token), token?.Length ?? 0);
            
            if (!string.IsNullOrEmpty(token))
            {
                logger.LogInformation("Token preview: {TokenPreview}...", 
                    token.Substring(0, Math.Min(30, token.Length)));
                Console.WriteLine($"Full token: {token}");
            }

            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Upload failed: Not authenticated");
                return Result<string>.Failure("You must be logged in to upload files");
            }

            // Opprett multipart form data
            logger.LogInformation("Creating multipart form data...");
            using var content = new MultipartFormDataContent();
            
            // Legg til fil
            logger.LogInformation("Adding file to form data...");
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 500 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);
            logger.LogInformation("File added: name={Name}, size={Size}", file.Name, file.Size);
            
            // Legg til version
            content.Add(new StringContent(version), "version");
            logger.LogInformation("Version added: {Version}", version);
            
            // Legg til platform som STRING
            var platformString = platform.ToString();
            content.Add(new StringContent(platformString), "platform");
            logger.LogInformation("Platform added: {Platform} (as string: {PlatformString})", platform, platformString);

            // Send request med Bearer token
            logger.LogInformation("Creating HTTP request...");
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;
            
            logger.LogInformation("Request URL: {Url}", request.RequestUri);
            logger.LogInformation("Authorization header: {Auth}", request.Headers.Authorization?.ToString());
            
            Console.WriteLine("=== ABOUT TO SEND REQUEST ===");
            Console.WriteLine($"URL: {request.RequestUri}");
            Console.WriteLine($"Method: {request.Method}");
            Console.WriteLine($"Auth header: {request.Headers.Authorization}");
            Console.WriteLine($"Content type: {content.Headers.ContentType}");

            logger.LogInformation("Sending request...");
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            logger.LogInformation("Response received: Status={StatusCode}", response.StatusCode);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Upload failed with {StatusCode}: {Error}", response.StatusCode, errorContent);
                Console.WriteLine($"Error response body: {errorContent}");
                return Result<string>.Failure($"Upload failed ({response.StatusCode}): {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("File uploaded successfully: {Result}", result);
            Console.WriteLine($"Success response: {result}");
            
            return Result<string>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error during file upload");
            Console.WriteLine($"Network exception: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            return Result<string>.Failure($"Connection failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during file upload");
            Console.WriteLine($"Unexpected exception: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Result<string>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}