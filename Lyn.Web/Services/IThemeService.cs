namespace Lyn.Web.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    event Action? OnThemeChanged;
    Task InitializeAsync();

    Task ToggleThemeAsync();

    Task ApplyThemeAsync();

    Task SetThemeAsync(string theme);
}