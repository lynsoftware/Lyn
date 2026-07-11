using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Features.DailyLog;
using Calorie.Core.Features.Library;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Features.DailyLog.ViewModels;

public partial class AddLogEntryViewModel : ObservableObject
{
    // ====== Tilstand ======
    
    // Listen med alle library items. Vises ikke til brukeren eller i UI-et
    private List<LibraryItem> _loadedItems = [];
    
    [ObservableProperty]
    private MealType _selectedMealType;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMealsTabActive), nameof(IsIngredientsTabActive),
        nameof(NewItemButtonText), nameof(EmptyHint))]
    private LibraryItemKind _activeKind = LibraryItemKind.Meal;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedItemDisplayName), nameof(KcalPreview),
        nameof(IsSelectionPanelVisible), nameof(CanCustomize))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmAddCommand))]
    private LibraryItem? _selectedLibraryItem;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KcalPreview))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmAddCommand))]
    private string _amountText = string.Empty;
    
    // Filtrert liste som er den som vises for brukeren
    [ObservableProperty]
    private List<LibraryItem> _filteredItems = [];
    
    // ===== Avledet tilstand — erstatter "Label.Text = ..." i code-behind =====
    
    // Name of selected chosen meal
    public string SelectedItemDisplayName => SelectedLibraryItem switch
    {
        null => string.Empty,
        { Brand: null } item => item.Name,
        { } item => $"{item.Name} ({item.Brand})"
    };
    
    public string KcalPreview => SelectedLibraryItem == null 
        ? string.Empty 
        : $"{SelectedLibraryItem.CaloriesFor(ParseAmount())} kcal";
    
    public bool IsSelectionPanelVisible => SelectedLibraryItem != null;
    public bool CanCustomize => SelectedLibraryItem?.Source is LibraryMeal;
    
    // ===== Commands — erstatter Clicked-handlers =====
    
    [RelayCommand]
    private void SelectMealType(MealType mealType) => SelectedMealType = mealType;
    
    [RelayCommand]
    private async Task LoadItemsAsync()
    { 
        _loadedItems = ActiveKind == LibraryItemKind.Meal
            ? (await LibraryStore.GetMealsAsync()).Select(LibraryItem.From).ToList()
            : (await LibraryStore.GetIngredientsAsync()).Select(LibraryItem.From).ToList();

        ApplyFilter();
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirmAdd))]
    private async Task ConfirmAddAsync()
    {
        await DayLogStore.AddItemAsync(SelectedMealType, SelectedLibraryItem!, ParseAmount());
        // TODO: Add navigation
    }
    
    private bool CanConfirmAdd() => SelectedLibraryItem != null && ParseAmount() > 0;

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnActiveKindChanged(LibraryItemKind value) => _ = LoadItemsAsync();
    
    private decimal ParseAmount()
    {
        var text = AmountText.Replace(',', '.');
        return decimal
            .TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var grams) && grams > 0
                ? grams
                : 0;
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

    /// <summary>
    /// Ved oppstart så auto velger vi måltidstype utifra klokkeslettet
    /// </summary>
    /// <param name="mealType"></param>
    public void Initialize(MealType? mealType) => SelectedMealType =
        mealType ?? MealTypeSuggester.SuggestFor(TimeOnly.FromDateTime(DateTime.Now));
    
    public bool IsMealsTabActive => ActiveKind == LibraryItemKind.Meal;
    public bool IsIngredientsTabActive = ActiveKind == LibraryItemKind.Ingredient;
    
    public string NewItemButtonText => ActiveKind == LibraryItemKind.Meal 
    ? $"+ {AppResources.NewMeal}"
    : $"+ {AppResources.NewIngredient}";
    
    public string EmptyHint => string.Format(AppResources.CreateItemHint, NewItemButtonText);
    
    [RelayCommand]
    private void SelectTab(LibraryItemKind kind) => ActiveKind = kind;


}