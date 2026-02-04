using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PasswordGenerator.Configuration;



namespace PasswordGenerator.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Changes the application language to English, persists the preference, and restarts the app
    /// to apply the new culture settings across all UI elements.
    /// </summary>
    private void OnEnglishClicked(object sender, EventArgs e)
    {
        var culture = new System.Globalization.CultureInfo("en");
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        System.Globalization.CultureInfo.CurrentCulture = culture;
        Preferences.Set(AppConstants.PreferenceKeyAppLanguage, "en");
        
        RestartApp();
    }
    
    /// <summary>
    /// Changes the application language to Norwegian, persists the preference, and restarts the app
    /// to apply the new culture settings across all UI elements.
    /// </summary>
    private void OnNorskClicked(object sender, EventArgs e)
    {
        var culture = new System.Globalization.CultureInfo("nb");
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        System.Globalization.CultureInfo.CurrentCulture = culture;
        Preferences.Set(AppConstants.PreferenceKeyAppLanguage, "nb"); 
        
        RestartApp();
    }
    
    /// <summary>
    /// Switches the application theme to light mode and persists the preference.
    /// The change takes effect immediately without requiring an app restart.
    /// </summary>
    private void OnLightThemeClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            Preferences.Set(AppConstants.PreferenceKeyAppTheme, "Light");
        }
    }
    
    /// <summary>
    /// Switches the application theme to dark mode and persists the preference.
    /// The change takes effect immediately without requiring an app restart.
    /// </summary>
    private void OnDarkThemeClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            Preferences.Set(AppConstants.PreferenceKeyAppTheme, "Dark");
        }
    }
    
    /// <summary>
    /// Restarts the application by recreating the main window with a fresh AppShell instance.
    /// Used after language changes to ensure all localized strings are properly updated.
    /// </summary>
    private void RestartApp()
    {
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell();
        }
    }
    
    /// <summary>
    /// Navigates back to the previous page (typically MainPage) by popping the current page from the navigation stack.
    /// </summary>
    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}