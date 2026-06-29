using Lyn.Backend.Platform.AppReleases.Repositories;
using Lyn.Backend.Platform.AppReleases.Services;
using Lyn.Backend.Platform.Auth.Services;
using Lyn.Backend.Platform.Support.Repositories;
using Lyn.Backend.Platform.Support.Services;

namespace Lyn.Backend.Startup;

/// <summary>
/// Samler registreringen av alle plattform-features (delt på tvers av alle apper).
/// Hver feature har sin egen private Add-metode slik at vi kan flytte registreringer
/// hit gruppe for gruppe, og senere trekke dem ut i egne modul-filer ved behov.
/// </summary>
public static class PlatformServiceExtensions
{
    /// <summary>
    /// Registrerer alle plattform-features. Kalles fra ConfigureServices i ServiceExtensions.
    /// </summary>
    public static IServiceCollection AddPlatform(this IServiceCollection services)
    {
        services.AddAppReleases();
        services.AddSupport();
        services.AddAuth();

        return services;
    }
    
    /// <summary>
    /// Auth — login og token-utstedelse (delt på tvers av alle apper).
    /// </summary>
    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();   // flyttes hit fra AddAuthInfrastructure
        return services;
    }

    /// <summary>
    /// AppReleases — delt release-/oppdateringsmekanisme for alle MAUI-apper.
    /// </summary>
    private static IServiceCollection AddAppReleases(this IServiceCollection services)
    {
        services.AddScoped<IAppReleaseService, AppReleaseService>();
        services.AddScoped<IAppReleaseRepository, AppReleaseRepository>();

        return services;
    }

    /// <summary>
    /// Support — supporthenvendelser fra alle apper (web og maui).
    /// </summary>
    private static IServiceCollection AddSupport(this IServiceCollection services)
    {
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddScoped<ISupportRepository, SupportRepository>();

        return services;
    }
}
