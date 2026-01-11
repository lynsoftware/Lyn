using Microsoft.Extensions.Logging;
using PasswordGenerator.Pages;
using PasswordGenerator.Services;

namespace PasswordGenerator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontAwesome");
                fonts.AddFont("Electrolize-Regular.ttf", "Electrolize");
            });
        
        // Services 
        builder.Services.AddSingleton<IPasswordService, PasswordService>();
        builder.Services.AddTransient<SettingsPage>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}