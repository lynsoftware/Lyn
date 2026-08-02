using Lyn.Backend.Apps.Calorie.Persistence;
using Lyn.Backend.Apps.Calorie.Sync.Repositories;
using Lyn.Backend.Apps.Calorie.Sync.Services;
using Lyn.Backend.Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Lyn.Backend.Startup.Modules;

/// <summary>
/// DI-registrering for Calorie-appen. Eier sin egen CalorieDbContext (samme
/// Postgres-database som AppDbContext, men egen migrasjonshistorikk), samlet i ett
/// inngangspunkt slik at modulen kan loftes ut til egen backend/database senere
/// uten a rore sentral wiring.
/// </summary>
public static class CalorieModule
{
    /// <summary>
    /// Registrerer Calorie-modulen. Kalles fra ConfigureServices i Startup.
    /// </summary>
    public static IServiceCollection AddCalorieModule(this IServiceCollection services)
    {
        // Database
        services.AddDbContext<CalorieDbContext>((sp, options) =>
        {
            var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            
            options.UseNpgsql(settings.DefaultConnection, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Calorie"));
            
        });
        
        // Services 
        services.AddScoped<ISyncService, SyncService>();
        
        // Repositories
        services.AddScoped<ISyncRepository, SyncRepository>();
        
        return services;
    }
}