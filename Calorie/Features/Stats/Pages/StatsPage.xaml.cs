using Calorie.Core.Features.Stats.ViewModels;

namespace Calorie.Features.Stats.Pages;

/// <summary>
/// Statistikken. Ren View: all tilstand og beregning bor i StatsViewModel.
/// Navbar-eventene videresendes til VM-kommandoene (BottomNavBar er event-basert).
/// </summary>
public partial class StatsPage : ContentPage
{
    private readonly StatsViewModel _vm;

    public StatsPage(StatsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadWeekCommand.Execute(null);
    }

    // ================== NAVBAR ==================

    private void OnHomeClicked(object sender, EventArgs e) => _vm.GoHomeCommand.Execute(null);

    private void OnMealsClicked(object sender, EventArgs e) => _vm.OpenLibraryCommand.Execute(null);

    private void OnLogClicked(object sender, EventArgs e) => _vm.OpenLogCommand.Execute(null);

    private void OnSettingsClicked(object sender, EventArgs e) => _vm.OpenSettingsCommand.Execute(null);
}
