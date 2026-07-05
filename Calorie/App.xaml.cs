using Calorie.Infrastructure.Constants;
using Calorie.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Calorie;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ThemeService.ApplySavedTheme();
        LoadLanguagePreference();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

#if WINDOWS || MACCATALYST
        window.MinimumWidth = 400;
        window.MinimumHeight = 600;

        window.MaximumWidth = 800;
        window.MaximumHeight = 1200;

#endif

        return window;
    }

    private void LoadLanguagePreference()
    {
        var savedLanguage = Preferences.Get(PreferenceKeys.AppLanguage, "en");
        var culture = new System.Globalization.CultureInfo(savedLanguage);
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        System.Globalization.CultureInfo.CurrentCulture = culture;
    }
}