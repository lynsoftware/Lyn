using Calorie.Core.Features.DailyLog.ViewModels;
using Calorie.Core.Features.MainPage.ViewModels;

namespace Calorie.Features.DailyLog.Pages;

/// <summary>
/// Hovedsiden — dagsloggen. Ren View: all tilstand og logikk bor i
/// MainPageViewModel. Navbar-eventene videresendes til VM-kommandoene
/// (BottomNavBar er event-basert).
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;

    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadDayCommand.Execute(null);
    }

    // ================== NAVBAR ==================

    private void OnAddClicked(object? sender, EventArgs e) => _vm.OpenLogFlowCommand.Execute(null);

    private void OnMealsClicked(object sender, EventArgs e) => _vm.OpenLibraryCommand.Execute(null);

    private void OnStatsClicked(object sender, EventArgs e) => _vm.OpenStatsCommand.Execute(null);

    private void OnSettingsClicked(object sender, EventArgs e) => _vm.OpenSettingsCommand.Execute(null);
}
