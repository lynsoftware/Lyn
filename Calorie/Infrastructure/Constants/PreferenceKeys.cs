namespace Calorie.Infrastructure.Constants;

/// <summary>
/// Nøkler til MAUIs Preferences-API — enhetsinnstillinger (tema, språk).
/// Hører hjemme i appen, ikke Core: Preferences finnes kun i MAUI, og
/// brukerens DATA (mål, logg, bibliotek) bor i SQLite via Core.
/// </summary>
public static class PreferenceKeys
{
    public const string AppLanguage = "AppLanguage";
    public const string AppTheme = "AppTheme";
}
