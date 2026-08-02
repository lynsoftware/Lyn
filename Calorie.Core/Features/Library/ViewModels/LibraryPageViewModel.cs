using Calorie.Core.Common;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.Library.ViewModels;

/// <summary>
/// ViewModel for biblioteket: faner (måltider/ingredienser), søk, og
/// navigasjon til editorene — trykk på rad åpner redigering, ny-knappen
/// følger aktiv fane. Navbar-navigasjonen eksponeres som kommandoer.
/// </summary>
public partial class LibraryPageViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ILibraryStore _library;

    private List<LibraryItem> _loadedItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMealsTabActive), nameof(IsIngredientsTabActive),
        nameof(NewItemButtonText), nameof(EmptyHint))]
    private LibraryItemKind _activeKind = LibraryItemKind.Meal;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private List<LibraryItem> _filteredItems = [];

    [ObservableProperty]
    private LibraryItem? _selectedItem;

    public LibraryPageViewModel(INavigationService navigation, ILibraryStore libraryStore)
    {
        _navigation = navigation;
        _library = libraryStore;
    }

    public bool IsMealsTabActive => ActiveKind == LibraryItemKind.Meal;

    public bool IsIngredientsTabActive => ActiveKind == LibraryItemKind.Ingredient;

    public string NewItemButtonText => ActiveKind == LibraryItemKind.Meal
        ? $"+ {AppResources.NewMeal}"
        : $"+ {AppResources.NewIngredient}";

    // Tomtilstandens handlingsforslag — følger aktiv fane (samme som logge-søket)
    public string EmptyHint => string.Format(AppResources.CreateItemHint, NewItemButtonText);

    // ===== Faner, søk og liste =====

    [RelayCommand]
    private void SelectTab(LibraryItemKind kind) => ActiveKind = kind;

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        _loadedItems = ActiveKind == LibraryItemKind.Meal
            ? (await _library.GetMealsAsync()).Select(LibraryItem.From).ToList()
            : (await _library.GetIngredientsAsync()).Select(LibraryItem.From).ToList();

        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnActiveKindChanged(LibraryItemKind value) => _ = LoadItemsAsync();

    /// <summary>
    /// Radtrykk åpner riktig editor. Valget nullstilles umiddelbart så samme
    /// rad kan trykkes igjen etter tilbake-navigering (re-entrant: null-settingen
    /// trigger kroken på nytt, som returnerer tidlig).
    /// </summary>
    partial void OnSelectedItemChanged(LibraryItem? value)
    {
        if (value == null)
            return;

        SelectedItem = null;

        _ = value.Source switch
        {
            LibraryMeal meal => _navigation.GoToEditMealAsync(meal),
            LibraryIngredient ingredient => _navigation.GoToEditIngredientAsync(ingredient),
            _ => Task.CompletedTask
        };
    }

    private void ApplyFilter()
    {
        var search = SearchText.Trim();

        FilteredItems = _loadedItems
            .Where(i => search.Length == 0
                        || i.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || i.Brand?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(i => i.Name)
            .ToList();
    }

    [RelayCommand]
    private Task NewItemAsync() => ActiveKind == LibraryItemKind.Meal
        ? _navigation.GoToMealBuilderAsync()
        : _navigation.GoToIngredientEditorAsync();

    // ===== Navbar =====

    [RelayCommand]
    private Task GoHomeAsync() => _navigation.GoHomeAsync();

    [RelayCommand]
    private Task OpenLogAsync() => _navigation.GoToAddLogEntryAsync();

    [RelayCommand]
    private Task OpenStatsAsync() => _navigation.GoToStatsAsync();

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToSettingsAsync();
}
