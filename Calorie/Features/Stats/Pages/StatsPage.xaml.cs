using Calorie.Core.Features.Stats.ViewModels;

namespace Calorie.Features.Stats.Pages;

/// <summary>
/// Statistikken. Ren View: all tilstand og beregning bor i StatsViewModel;
/// navbaren binder rett til VM-kommandoene.
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
}
