using Calorie.Core.Features.Settings.ViewModels;

namespace Calorie.Features.Settings.Pages;

/// <summary>
/// Innstillinger. Ren View: tema-/språkvalg og navigasjon bor i
/// SettingsViewModel (via IAppearanceService/INavigationService).
/// </summary>
public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
