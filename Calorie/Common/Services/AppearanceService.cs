using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Services;
using Calorie.Infrastructure.Constants;
using Calorie.Infrastructure.Services;

namespace Calorie.Common.Services;

/// <summary>
/// Tema- og språkbytte: ThemeService/Preferences/CultureInfo + gjenskaping
/// av AppShell slik at kodegenererte farger og lokaliserte strenger tegnes
/// på nytt (samme mønster for begge byttene).
/// </summary>
public class AppearanceService : IAppearanceService
{
    public bool IsLightTheme =>
        Preferences.Get(PreferenceKeys.AppTheme, ThemeService.DarkGold) == ThemeService.LightGold;

    public bool IsNorwegian => Preferences.Get(PreferenceKeys.AppLanguage, "en") == "nb";

    public void ApplyTheme(bool lightTheme)
    {
        ThemeService.Apply(lightTheme ? ThemeService.LightGold : ThemeService.DarkGold);
        RestartShell();
    }

    public void ApplyLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Preferences.Set(PreferenceKeys.AppLanguage, cultureCode);

        RestartShell();
    }

    private static void RestartShell()
    {
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new AppShell();
    }
}
