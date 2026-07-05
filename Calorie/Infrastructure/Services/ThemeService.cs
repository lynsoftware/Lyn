using Calorie.Infrastructure.Constants;
using Calorie.Resources.Themes;

namespace Calorie.Infrastructure.Services;

/// <summary>
/// Bytter apptema ved å swappe tema-ordboken i MergedDictionaries.
/// All XAML refererer semantiske tokens via DynamicResource (oppdateres live);
/// code-behind leser via GetColor. Nytt tema = ny ResourceDictionary med
/// samme token-nøkler + en linje i Apply.
/// </summary>
public static class ThemeService
{
    public const string DarkGold = "DarkGold";
    public const string LightGold = "LightGold";

    private static ResourceDictionary? _currentTheme;

    /// <summary>
    /// Brukes ved oppstart — leser lagret valg (standard: mørkt).
    /// </summary>
    public static void ApplySavedTheme() => Apply(Preferences.Get(PreferenceKeys.AppTheme, DarkGold));

    public static void Apply(string themeName)
    {
        var app = Application.Current;
        if (app == null)
            return;

        // "Light"/"Dark" er gamle lagrede verdier fra UserAppTheme-systemet
        var isLight = themeName is LightGold or "Light";

        ResourceDictionary theme = isLight ? new LightGoldTheme() : new DarkGoldTheme();

        if (_currentTheme != null)
            app.Resources.MergedDictionaries.Remove(_currentTheme);
        app.Resources.MergedDictionaries.Add(theme);
        _currentTheme = theme;

        // Holder MAUIs innebygde Light/Dark i synk (standardstyles, systemfelter)
        app.UserAppTheme = isLight ? AppTheme.Light : AppTheme.Dark;

        Preferences.Set(PreferenceKeys.AppTheme, isLight ? LightGold : DarkGold);
    }

    /// <summary>
    /// Slår opp en semantisk temafarge fra code-behind (der DynamicResource
    /// ikke kan brukes). Magenta = manglende token, med vilje synlig.
    /// </summary>
    public static Color GetColor(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.Magenta;
}
