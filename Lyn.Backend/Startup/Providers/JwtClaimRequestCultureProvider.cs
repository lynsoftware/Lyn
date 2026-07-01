using Microsoft.AspNetCore.Localization;

namespace Lyn.Backend.Startup.Providers;

/// <summary>
/// Leser "lang"-claimen fra det autentiserte JWT-et og bruker den som request-kultur.
/// Returnerer null når brukeren er anonym eller claimen mangler, slik at neste
/// provider (Accept-Language) tar over.
/// </summary>
public class JwtClaimRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var lang = httpContext.User.FindFirst("lang")?.Value;

        return string.IsNullOrWhiteSpace(lang)
            ? NullProviderCultureResult
            : Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(lang));
    }
}