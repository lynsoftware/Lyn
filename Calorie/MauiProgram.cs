using Calorie.Features.Settings.Pages;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;


namespace Calorie;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.WriteLine($"UNHANDLED: {args.ExceptionObject}");
        };

        // Lokal SQLite i appens sandkasse — migreres og seedes ved oppstart
        Calorie.Core.Data.LocalDb.Initialize(Path.Combine(FileSystem.AppDataDirectory, "calorie.db3"));
        
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontAwesome");
                fonts.AddFont("Electrolize-Regular.ttf", "Electrolize");
            });
        
        // Services — Calorie-tjenestene (lokal lagring, sync, beregning) registreres her etter hvert
        builder.Services.AddTransient<SettingsPage>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}