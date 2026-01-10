using System.Globalization;
using Blazored.LocalStorage;

namespace Lyn.Web.Services;

public class LocalizationService(ILocalStorageService localStorage) : ILocalizationService
{
    private const string LanguageKey = "selected-language";
    private string _currentLanguage = "en";

    public event Action? OnLanguageChanged;
    public string CurrentLanguage => _currentLanguage;

    public async Task InitializeAsync()
    {
        try
        {
            var savedLanguage = await localStorage.GetItemAsStringAsync(LanguageKey);
            _currentLanguage = string.IsNullOrEmpty(savedLanguage) ? "en" : savedLanguage.Trim('"');
            
            SetCulture(_currentLanguage);
        }
        catch
        {
            _currentLanguage = "en";
            SetCulture("en");
        }
    }

    public async Task SetLanguageAsync(string culture)
    {
        if (_currentLanguage == culture)
            return;

        _currentLanguage = culture;
        await localStorage.SetItemAsStringAsync(LanguageKey, culture);
        
        SetCulture(culture);
        
        // Viktig: Trigger event ETTER kultur er satt
        OnLanguageChanged?.Invoke();
    }

    private void SetCulture(string culture)
    {
        var cultureInfo = culture == "no" 
            ? new CultureInfo("nb-NO") 
            : new CultureInfo("en-US");
        
        // Sett ALLE kultur-properties for å sikre at endringen gjennomføres
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        
        Console.WriteLine($@"Culture set to: {CultureInfo.CurrentUICulture.Name}");
        Console.WriteLine($@"DefaultThreadCurrentUICulture: {CultureInfo.DefaultThreadCurrentUICulture?.Name}");
    }
}