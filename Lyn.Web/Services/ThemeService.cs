using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace Lyn.Web.Services;

public class ThemeService(ILocalStorageService localStorage, IJSRuntime jsRuntime) : IThemeService
{
    private const string ThemeKey = "selected-theme";
    private string _currentTheme = "light";

    public event Action? OnThemeChanged;
    public string CurrentTheme => _currentTheme;

    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await localStorage.GetItemAsStringAsync(ThemeKey);
            
            if (string.IsNullOrEmpty(savedTheme))
            {
                // Første gang - bruk system-preferanse
                _currentTheme = await jsRuntime.InvokeAsync<string>("getPreferredTheme");
            }
            else
            {
                // Fjern quotes hvis de finnes (Blazored.LocalStorage kan legge til quotes)
                _currentTheme = savedTheme.Trim('"');
            }
        }
        catch
        {
            _currentTheme = "light";
        }
    }

    public async Task ToggleThemeAsync()
    {
        _currentTheme = _currentTheme == "light" ? "dark" : "light";
        await localStorage.SetItemAsStringAsync(ThemeKey, _currentTheme);
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    public async Task ApplyThemeAsync()
    {
        Console.WriteLine($"ApplyThemeAsync: Setting data-theme to {_currentTheme}");
        await jsRuntime.InvokeVoidAsync("setTheme", _currentTheme);
    }
    
    public async Task SetThemeAsync(string theme)
    {
        Console.WriteLine($"SetThemeAsync called with: {theme}");
    
        if (_currentTheme != theme && (theme == "light" || theme == "dark"))
        {
            _currentTheme = theme;
            Console.WriteLine($"Theme changed to: {_currentTheme}");
        
            await localStorage.SetItemAsStringAsync(ThemeKey, _currentTheme);
            await ApplyThemeAsync();
            OnThemeChanged?.Invoke();
        }
    }
}