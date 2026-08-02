using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Biblioteket. Ren View: all tilstand og logikk bor i LibraryPageViewModel;
/// navbaren binder rett til VM-kommandoene.
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
}
