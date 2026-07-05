using Calorie.Features.Goals.Pages;
using System.Globalization;
using Calorie.Infrastructure.Constants;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Settings.Pages;

/// <summary>
/// Innstillinger: mål (egen side), tema (ThemeService) og språk. Tema- og
/// språkbytte gjenskaper AppShell slik at kodegenererte farger og
/// lokaliserte strenger tegnes på nytt.
/// </summary>
public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateSelectionStyles();
    }

    // ================== MÅL ==================

    private async void OnGoalsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new GoalSettingsPage());

    // ================== TEMA ==================

    private void OnLightThemeClicked(object sender, EventArgs e) => ApplyTheme(ThemeService.LightGold);

    private void OnDarkThemeClicked(object sender, EventArgs e) => ApplyTheme(ThemeService.DarkGold);

    private static void ApplyTheme(string themeName)
    {
        ThemeService.Apply(themeName);
        RestartShell();
    }

    // ================== SPRÅK ==================

    private void OnNorwegianClicked(object sender, EventArgs e) => ApplyLanguage("nb");

    private void OnEnglishClicked(object sender, EventArgs e) => ApplyLanguage("en");

    private static void ApplyLanguage(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Preferences.Set(PreferenceKeys.AppLanguage, cultureCode);

        RestartShell();
    }

    // ================== FELLES ==================

    /// <summary>
    /// Gjenskaper AppShell slik at alle sider bygges på nytt med gjeldende
    /// tema og språk — samme mønster for begge byttene.
    /// </summary>
    private static void RestartShell()
    {
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new AppShell();
    }

    /// <summary>
    /// Markerer gjeldende tema- og språkvalg med primærfargen.
    /// </summary>
    private void UpdateSelectionStyles()
    {
        var primary = ThemeService.GetColor("Primary");
        var onPrimary = ThemeService.GetColor("OnPrimary");
        var surface = ThemeService.GetColor("Surface");
        var secondary = ThemeService.GetColor("TextSecondary");

        var isLightTheme = Preferences.Get(PreferenceKeys.AppTheme, ThemeService.DarkGold) == ThemeService.LightGold;
        LightThemeButton.BackgroundColor = isLightTheme ? primary : surface;
        LightThemeButton.TextColor = isLightTheme ? onPrimary : secondary;
        DarkThemeButton.BackgroundColor = isLightTheme ? surface : primary;
        DarkThemeButton.TextColor = isLightTheme ? secondary : onPrimary;

        var isNorwegian = Preferences.Get(PreferenceKeys.AppLanguage, "en") == "nb";
        NorwegianButton.BackgroundColor = isNorwegian ? primary : surface;
        NorwegianButton.TextColor = isNorwegian ? onPrimary : secondary;
        EnglishButton.BackgroundColor = isNorwegian ? surface : primary;
        EnglishButton.TextColor = isNorwegian ? secondary : onPrimary;
    }

    /// <summary>
    /// Navigates back to the previous page by popping the current page from the navigation stack.
    /// </summary>
    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
