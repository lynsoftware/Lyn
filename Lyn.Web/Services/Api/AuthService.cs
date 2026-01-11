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
            logger.LogInformation("Uploading file: {FileName}, Version: {Version}, Platform: {Platform}", 
                file.Name, version, platform);

            // Hent token
            var token = await GetTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Upload failed: Not authenticated");
                return Result<string>.Failure("You must be logged in to upload files");
            }

            // Opprett multipart form data
            using var content = new MultipartFormDataContent();
            
            // Legg til fil
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 500 * 1024 * 1024)); // 500MB max
            
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) 
                ? GetContentTypeFromExtension(file.Name)
                : file.ContentType;

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", file.Name);
            
            // Legg til version
            content.Add(new StringContent(version), "version");
            
            // Legg til platform (som integer)
            content.Add(new StringContent(((int)platform).ToString()), "platform");

            // Send request med Bearer token
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Upload failed: {Error}", errorContent);
                Console.WriteLine($@"Full error response: {errorContent}");
                return Result<string>.Failure(errorContent);
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("File uploaded successfully: {Result}", result);
            
            return Result<string>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Network error during file upload");
            return Result<string>.Failure("Connection failed. Please check your internet.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during file upload");
            return Result<string>.Failure("Unexpected error occurred. Try again later.");
        }
    }
    
    private static string GetContentTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
    
        return extension switch
        {
            ".apk" => "application/vnd.android.package-archive",
            ".exe" => "application/vnd.microsoft.portable-executable",
            ".msi" => "application/x-msi",
            ".dmg" => "application/x-apple-diskimage",
            ".zip" => "application/zip",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            _ => "application/octet-stream" // Default for ukjente filtyper
        };
    }
}