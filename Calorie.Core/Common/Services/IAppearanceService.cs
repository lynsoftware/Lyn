namespace Calorie.Core.Common.Services;

/// <summary>
/// Tema- og språkvalg sett fra ViewModels. Appen implementerer med
/// ThemeService/Preferences/CultureInfo + AppShell-rebuild — alt sammen
/// plattformdetaljer som ikke hører hjemme i Core.
/// </summary>
public interface IAppearanceService
{
    bool IsLightTheme { get; }

    bool IsNorwegian { get; }

    void ApplyTheme(bool lightTheme);

    void ApplyLanguage(string cultureCode);
}
