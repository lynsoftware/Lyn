using Calorie.Core.Features.Goals.ViewModels;

namespace Calorie.Features.Goals.Pages;

/// <summary>
/// Mål-oppsettet. Ren View: all tilstand og logikk bor i GoalSettingsViewModel.
/// </summary>
public partial class GoalSettingsPage : ContentPage
{
    private readonly GoalSettingsViewModel _vm;

    public GoalSettingsPage(GoalSettingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }
}
