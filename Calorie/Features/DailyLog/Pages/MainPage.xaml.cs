using Calorie.Core.Features.MainPage.ViewModels;

namespace Calorie.Features.DailyLog.Pages;

/// <summary>
/// Hovedsiden — dagsloggen. Ren View: all tilstand og logikk bor i
/// MainPageViewModel; navbaren binder rett til VM-kommandoene.
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
}
