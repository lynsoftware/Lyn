using Microsoft.Extensions.DependencyInjection;

namespace PasswordGenerator;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        LoadThemePreference();
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

    private void LoadThemePreference()
    {
        string savedTheme = Preferences.Get("AppTheme", "Light");
        UserAppTheme = savedTheme == "Dark" ? AppTheme.Dark : AppTheme.Light;
    }

    private void LoadLanguagePreference()
    {
        var savedLanguage = Preferences.Get("AppLanguage", "en");
        var culture = new System.Globalization.CultureInfo(savedLanguage);
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        System.Globalization.CultureInfo.CurrentCulture = culture;
    }
}