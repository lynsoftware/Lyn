using Lyn.Backend.Apps.PasswordGenerator.Repositories;
using Lyn.Backend.Apps.PasswordGenerator.Services;

namespace Lyn.Backend.Startup.Modules;

/// <summary>
/// DI-registrering for PasswordGenerator-appen. Eier alle tjenester og repositories
/// som hører til dette produktet, samlet i ett inngangspunkt slik at modulen kan
/// løftes ut til egen backend senere uten å røre sentral wiring.
/// </summary>
public static class PasswordGeneratorModule
{
    /// <summary>
    /// Registrerer PasswordGenerator-modulen. Kalles fra ConfigureServices i Startup.
    /// </summary>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();

        // Repositories
        services.AddScoped<IPasswordGeneratorStatisticsRepository, PasswordGeneratorStatisticsRepository>();

        return services;
    }
}
