using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Måltidsbyggeren. Ren View: all tilstand og logikk bor i
/// MealBuilderViewModel — siden oversetter kun Shell-parametere
/// (ny/rediger/tilpass + nyopprettet ingrediens) til typede VM-kall.
/// </summary>
public partial class MealBuilderPage : ContentPage, IQueryAttributable
{
    private readonly MealBuilderViewModel _vm;

    public MealBuilderPage(MealBuilderViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadIngredientsCommand.Execute(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavKeys.InitialName, out var name) && name is string initialName)
            _vm.SetInitialName(initialName);

        if (query.TryGetValue(NavKeys.MealToEdit, out var toEdit) && toEdit is LibraryMeal meal)
            _vm.LoadExisting(meal);

        if (query.TryGetValue(NavKeys.MealToCustomize, out var toCustomize) && toCustomize is LibraryMeal customize)
            _vm.LoadForCustomize(customize);

        if (query.TryGetValue(NavKeys.CreatedItemId, out var created) && created is Guid id)
            _vm.NotifyIngredientCreated(id);
    }
}
