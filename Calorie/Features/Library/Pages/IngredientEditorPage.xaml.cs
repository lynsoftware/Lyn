using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Ingrediens-editoren. Ren View: all tilstand og logikk bor i
/// IngredientEditorViewModel — siden oversetter kun Shell-parametere
/// til typede VM-kall.
/// </summary>
public partial class IngredientEditorPage : ContentPage, IQueryAttributable
{
    private readonly IngredientEditorViewModel _vm;

    public IngredientEditorPage(IngredientEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavKeys.InitialName, out var name) && name is string initialName)
            _vm.SetInitialName(initialName);

        if (query.TryGetValue(NavKeys.IngredientToEdit, out var existing) && existing is LibraryIngredient ingredient)
            _vm.LoadExisting(ingredient);
    }
}
