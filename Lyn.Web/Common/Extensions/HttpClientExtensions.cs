using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Lyn.Shared.Enum;
using Lyn.Shared.Result;

namespace Lyn.Web.Common.Extensions;

/// <summary>
/// Extensions for å lese HTTP-responser fra backend og gjøre dem om til Result med AppErrorCode.
/// Én kilde til feil-parsing for alle API-tjenester i frontend.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Void-endepunkter: tom 200 OK => Success, ellers Failure med AppProblemDetails fra responsen.
    /// </summary>
    public static async Task<Result> ParseEmptyResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var (message, code) = await ReadProblemDetailAsync(response, ct);
            return Result.Failure(message, code);
        }

        return Result.Success();
    }

    /// <summary>
    /// Endepunkter med verdi: 2xx => deserialiser body, ellers Failure med AppProblemDetails fra responsen.
    /// </summary>
    public static async Task<Result<T>> ParseResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var (message, code) = await ReadProblemDetailAsync(response, ct);
            return Result<T>.Failure(message, code);
        }

        try
        {
            T? body = await response.Content.ReadFromJsonAsync<T>(ct);

            // Kan kun skje hvis backend sender "null" med suksess-status
            if (body is null)
                return Result<T>.Failure("Server returnerte suksess og null i body", AppErrorCode.Unknown);

            return Result<T>.Success(body);
        }
        catch (JsonException)
        {
            return Result<T>.Failure("Kunne ikke lese JSON-respons fra server", AppErrorCode.Unknown);
        }
    }

    /// <summary>
    /// Leser vårt AppProblemDetails (detail + code) fra en feil-respons. Håndterer også ASP.NET
    /// sin model-validation (errors-dict) og faller til Unknown hvis kroppen ikke kan tolkes.
    /// </summary>
    private static async Task<(string Message, AppErrorCode Code)> ReadProblemDetailAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return ("Du har ikke tilgang til denne ressursen.", AppErrorCode.Forbidden);

            // Leser hele kroppen som et JsonDocument for å skille model-validation fra vår egen feil
            using JsonDocument doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            JsonElement root = doc.RootElement;

            // ASP.NET model-validation (400) => "errors"-dict
            if (root.TryGetProperty("errors", out JsonElement errors))
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(field => field.Value.EnumerateArray()
                        .Select(v => v.GetString() ?? string.Empty))
                    .Where(message => !string.IsNullOrEmpty(message));

                return (string.Join(" ", messages), AppErrorCode.Validation);
            }

            // Vår AppProblemDetails: detail (melding) + code (int)
            string? detail = root.TryGetProperty("detail", out JsonElement d) ? d.GetString() : null;

            AppErrorCode code = root.TryGetProperty("code", out JsonElement c) && c.TryGetInt32(out int codeValue)
                ? (AppErrorCode)codeValue
                : AppErrorCode.Unknown;

            return (detail ?? "Ukjent feil fra serveren", code);
        }
        catch (JsonException)
        {
            return ("Ukjent feil fra serveren", AppErrorCode.Unknown);
        }
    }
}
