namespace Lyn.Web.Services;

public interface ILocalizationService
{
    event Action? OnLanguageChanged;
    Task InitializeAsync();
    Task SetLanguageAsync(string culture);
    string CurrentLanguage { get; }
}