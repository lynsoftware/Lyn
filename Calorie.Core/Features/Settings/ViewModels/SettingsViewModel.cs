using Calorie.Core.Common;
using Calorie.Core.Common.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.Settings.ViewModels;

/// <summary>
/// ViewModel for innstillinger: mål-inngang, tema- og språkvalg.
/// Tema/språk gjenskaper AppShell (via IAppearanceService), så tilstanden
/// her lever bare til siden bygges på nytt med nye verdier.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IAppearanceService _appearance;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    private bool _isLightTheme;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnglish))]
    private bool _isNorwegian;

    public SettingsViewModel(IAppearanceService appearance, INavigationService navigation)
    {
        _appearance = appearance;
        _navigation = navigation;

        IsLightTheme = appearance.IsLightTheme;
        IsNorwegian = appearance.IsNorwegian;
    }

    public bool IsDarkTheme => !IsLightTheme;

    public bool IsEnglish => !IsNorwegian;

    // ===== Commands =====

    [RelayCommand]
    private void SelectLightTheme() => _appearance.ApplyTheme(lightTheme: true);

    [RelayCommand]
    private void SelectDarkTheme() => _appearance.ApplyTheme(lightTheme: false);

    [RelayCommand]
    private void SelectNorwegian() => _appearance.ApplyLanguage("nb");

    [RelayCommand]
    private void SelectEnglish() => _appearance.ApplyLanguage("en");

    [RelayCommand]
    private Task OpenGoalsAsync() => _navigation.GoToGoalSettingsAsync();

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();
}
