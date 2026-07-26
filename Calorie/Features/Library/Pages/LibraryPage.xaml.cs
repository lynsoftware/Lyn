using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Biblioteket. Ren View: all tilstand og logikk bor i LibraryPageViewModel.
/// Navbar-eventene videresendes til VM-kommandoene (BottomNavBar er event-basert).
/// </summary>
public partial class LibraryPage : ContentPage
{
    private readonly LibraryPageViewModel _vm;

    public LibraryPage(LibraryPageViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadItemsCommand.Execute(null);
    }

    // ================== NAVBAR ==================

    private void OnHomeClicked(object sender, EventArgs e) => _vm.GoHomeCommand.Execute(null);

    private void OnLogClicked(object sender, EventArgs e) => _vm.OpenLogCommand.Execute(null);

    private void OnStatsClicked(object sender, EventArgs e) => _vm.OpenStatsCommand.Execute(null);

    private void OnSettingsClicked(object sender, EventArgs e) => _vm.OpenSettingsCommand.Execute(null);
}
