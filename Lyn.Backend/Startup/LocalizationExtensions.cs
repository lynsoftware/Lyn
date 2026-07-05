using System.Globalization;
using Lyn.Backend.Platform.Localization;
using Lyn.Backend.Startup.Providers;
using Microsoft.AspNetCore.Localization;

namespace Lyn.Backend.Startup;

public static class LocalizationExtensions
{
    /// <summary>
    /// Delt localization-maskineri: resx-baserte IStringLocalizer + culture-resolving
    /// fra JWT-claim (autentiserte) med Accept-Language som fallback for ikke autentisert brukere.
    /// Ressursene eies per feature/app (egne .resx ved siden av markørklassen).
    /// </summary>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        // Tom ResourcesPath => resx forventes ved siden av markørklassen (samme namespace).
        services.AddLocalization();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = SupportedCultures.Codes.Select(c => new CultureInfo(c)).ToList();

            options.DefaultRequestCulture = new RequestCulture(SupportedCultures.Default);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;

            // Rekkefølge: JWT-claim -> Accept-Language -> default.
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new JwtClaimRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });

        return services;
    }
}