using System.Net.Http.Json;
using Lyn.Shared.Enum;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;
using Lyn.Web.Common.Extensions;

namespace Lyn.Web.Services.Api;

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

            // ParseResponseAsync håndterer feilkode, null-body og JSON-feil
            return await HttpClientExtensions.ParseResponseAsync<PasswordGenerationResponse>(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when generating password. Request: {@Payload}", request);
            return Result<PasswordGenerationResponse>.Failure("Could not generate password", AppErrorCode.Unknown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error when generating password");
            return Result<PasswordGenerationResponse>.Failure("Unexpected error occurred", AppErrorCode.Unknown);
        }
    }
}
