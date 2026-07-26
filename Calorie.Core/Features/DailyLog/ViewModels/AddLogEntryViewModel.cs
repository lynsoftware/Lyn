using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Extensions;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.DailyLog.Services;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Features.Library.ViewModels;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.DailyLog.ViewModels;

public partial class AddLogEntryViewModel : ObservableObject
{
    // ====== Egenskaper ======
    private readonly INavigationService _navigation;
    private readonly IToastService _toast;
    private readonly ILibraryStore _library;
    private readonly IDayLogStore _dayLog;
    private readonly TimeProvider _time;

    public IReadOnlyList<MealTypeOption> MealTypeOptions { get; }
    
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
    private LibraryItem? _selectedListItem;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveItemDisplayName), nameof(KcalPreview),
        nameof(IsSelectionPanelVisible), nameof(CanCustomize), nameof(UnitOptions))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmAddCommand), nameof(CustomizeCommand))]
    private LibraryItem? _activeItem;

    private bool _activeIsCustomized;
    
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

    public string ActiveItemDisplayName => ActiveItem switch
    {
        null => string.Empty,
        _ when _activeIsCustomized => $"{ActiveItem.Name} ({AppResources.CustomizedLabel})",
        { Brand: null } item => item.Name,
        { } item => $"{item.Name} ({item.Brand})"
    };
    
    
    public string KcalPreview => ActiveItem == null 
        ? string.Empty 
        : $"{ActiveItem.CaloriesFor(ParseAmount())} kcal";
    
    public bool IsSelectionPanelVisible => ActiveItem != null;
    public bool CanCustomize => ActiveItem?.Source is LibraryMeal;

    public AddLogEntryViewModel(
        INavigationService navigationService,
        IToastService toastService,
        ILibraryStore libraryStore,
        IDayLogStore dayLogStore,
        TimeProvider timeProvider)
    {
        _navigation = navigationService;
        _toast = toastService;
        _library = libraryStore;
        _dayLog = dayLogStore;
        _time = timeProvider;

        _logDate = Today;

        MealTypeOptions = Enum.GetValues<MealType>()
            .OrderBy(t => t.DisplayOrder())
            .Select(t => new MealTypeOption(t, t.ToDisplayName()))
            .ToList();

        SelectedMealType = MealTypeSuggester.SuggestFor(TimeOnly.FromDateTime(_time.GetLocalNow().DateTime));
        SyncMealTypeOptions();
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
    
    // ===== Commands — erstatter Clicked-handlers =====
    
    [RelayCommand]
    private void SelectMealType(MealTypeOption option) => SelectedMealType = option.Value;
    
    [RelayCommand]
    private async Task LoadItemsAsync()
    { 
        _loadedItems = ActiveKind == LibraryItemKind.Meal
            ? (await _library.GetMealsAsync()).Select(LibraryItem.From).ToList()
            : (await _library.GetIngredientsAsync()).Select(LibraryItem.From).ToList();

        ApplyFilter();
        TrySelectPendingCreated();
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirmAdd))]
    private async Task ConfirmAddAsync()
    {
        await _dayLog.AddItemAsync(SelectedMealType, ActiveItem!, ParseAmount(), _logDate);
        await _toast.ShowSuccessAsync(AppResources.ItemLogged);
        await _navigation.GoBackAsync();
    }
    
    private bool CanConfirmAdd() => ActiveItem != null && ParseAmount() > 0;

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnActiveKindChanged(LibraryItemKind value) => _ = LoadItemsAsync();

    partial void OnActiveItemChanged(LibraryItem? value) => AmountText =
        value?.DefaultAmountGrams.ToString("0.#", CultureInfo.CurrentCulture) ?? string.Empty;

    partial void OnSelectedMealTypeChanged(MealType value) => SyncMealTypeOptions();

    private void SyncMealTypeOptions()
    {
        foreach (var option in MealTypeOptions)
            option.IsSelected = option.Value == SelectedMealType;
    }
    
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
    
  
    
    public bool IsMealsTabActive => ActiveKind == LibraryItemKind.Meal;
    public bool IsIngredientsTabActive => ActiveKind == LibraryItemKind.Ingredient;
    
    public string NewItemButtonText => ActiveKind == LibraryItemKind.Meal 
    ? $"+ {AppResources.NewMeal}"
    : $"+ {AppResources.NewIngredient}";
    
    public string EmptyHint => string.Format(AppResources.CreateItemHint, NewItemButtonText);
    
    [RelayCommand]
    private void SelectTab(LibraryItemKind kind) => ActiveKind = kind;

    partial void OnSelectedListItemChanged(LibraryItem? value)
    {
        // Kun ekte valg følges — null fra gjenlasting skal ikke rive panelet
        if (value != null)
            SetActiveItem(value, customized: false);
    }

    private void SetActiveItem(LibraryItem item, bool customized)
    {
        _activeIsCustomized = customized;   // settes FØR ActiveItem — DisplayName leser det
        ActiveItem = item;
    }

    public IReadOnlyList<UnitOption> UnitOptions
    {
        get
        {
            if (ActiveItem is not { UnitWeightGrams: { } weight, UnitName: { } name } || weight <= 0)
                return [];
            
            return
            [
                new UnitOption($"1 {name}", weight),
                new UnitOption($"2 {name}", weight * 2)
            ];
        }
    }

    [RelayCommand]
    private void SetUnitAmount(UnitOption option) =>
        AmountText = option.Grams.ToString("0.#", CultureInfo.CurrentCulture);

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();
    
    [RelayCommand]
    private Task ScanAsync() => _toast.ShowNoticeAsync(AppResources.ScanComingSoon);

    [RelayCommand]
    private Task NewItemAsync()
    {
        var search = SearchText.Trim();
        var initialName = search.Length == 0 ? null : search;

        return ActiveKind == LibraryItemKind.Meal
            ? _navigation.GoToMealBuilderAsync(initialName)
            : _navigation.GoToIngredientEditorAsync(initialName);
    }

    [RelayCommand(CanExecute = nameof(CanCustomize))]
    private Task CustomizeAsync() => _navigation.GoToCustomizeMealAsync((LibraryMeal)ActiveItem!.Source!);
    
    // Vare opprettet i en editor — plukkes opp og velges ved neste lasting
    private Guid? _pendingCreatedId;

    // Dagen det logges til — settes i konstruktøren (i dag) og overstyres
    // fra visningsdatoen på hovedsiden
    private DateOnly _logDate;

    /// <summary>"Logg mat" på dagens dato — ellers "Logg mat — fre 25. jul".</summary>
    public string LogTitle
    {
        get
        {
            var today = Today;
            return _logDate == today
                ? AppResources.LogFoodTitle
                : $"{AppResources.LogFoodTitle} — {_logDate.ToString("ddd d. MMM", CultureInfo.CurrentCulture)}";
        }
    }

    public void SetLogDate(DateOnly date)
    {
        _logDate = date;
        OnPropertyChanged(nameof(LogTitle));
    }

    public void SetMealType(MealType mealType) => SelectedMealType = mealType;

    public void NotifyItemCreated(Guid id, LibraryItemKind? kind = null)
    {
        _pendingCreatedId = id;
        if (kind is { } k)
            ActiveKind = k;
    }

    public void ApplyCustomizeMeal(LibraryMeal meal)
    {
        SelectedListItem = null;
        SetActiveItem(LibraryItem.From(meal), customized: true);
    }

    private void TrySelectPendingCreated()
    {
        if (_pendingCreatedId is not { } id)
            return;

        _pendingCreatedId = null;
        SearchText = string.Empty;

        var created = _loadedItems.FirstOrDefault(i => i.Source switch
        {
            LibraryIngredient ingredient => ingredient.Id == id,
            LibraryMeal meal => meal.Id == id,
            _ => false
        });

        if (created != null)
            SelectedListItem = created;
    }
    
}