using Calorie.Core.Common;
using System.Globalization;
using Calorie.Core.Features.DailyLog;
using Calorie.Core.Features.Library;
using Calorie.Core.Resources.Strings;
using Calorie.Features.Library.Pages;
using Calorie.Infrastructure.Services;
using CommunityToolkit.Maui.Alerts;

namespace Calorie.Features.DailyLog.Pages;

/// <summary>
/// Logge-flyten: velg måltidstype, finn vare i biblioteket, angi mengde,
/// legg til i dagsloggen (DayLogStore i Fase 4A — SQLite i 4B).
/// </summary>
public partial class AddLogEntryPage : ContentPage
{
    private readonly Dictionary<MealType, Button> _chips = new();

    private MealType _selectedMealType;
    private LibraryItemKind _activeKind = LibraryItemKind.Meal;
    private LibraryItem? _selectedItem;
    private List<LibraryItem> _loadedItems = [];

    // Id-en til en vare opprettet fra denne flyten — forhåndsvelges ved retur
    private Guid? _createdId;

    // Tilpasset engangs-kopi av et måltid (finnes ikke i biblioteket) — velges ved retur
    private LibraryMeal? _customizedMeal;

    /// <param name="mealType">Forhåndsvalgt måltidstype (fra seksjonens legg-til)
    /// — null gir klokkeslett-basert forslag (pluss-knappen).</param>
    public AddLogEntryPage(MealType? mealType = null)
    {
        InitializeComponent();

        _selectedMealType = mealType ?? MealTypeSuggester.SuggestFor(TimeOnly.FromDateTime(DateTime.Now));
        AddConfirmButton.Text = AppResources.AddFood;

        BuildMealTypeChips();
        UpdateTabStyles();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadItemsAsync();
    }

    // ================== MÅLTIDSTYPE-CHIPS ==================

    private void BuildMealTypeChips()
    {
        foreach (var type in Enum.GetValues<MealType>().OrderBy(t => t.DisplayOrder()))
        {
            var chip = new Button
            {
                Text = type.ToDisplayName(),
                FontSize = 13,
                Padding = new Thickness(14, 6),
                CornerRadius = 16
            };
            chip.Clicked += (_, _) =>
            {
                _selectedMealType = type;
                StyleChips();
            };

            _chips[type] = chip;
            ChipsContainer.Children.Add(chip);
        }

        StyleChips();
    }

    private void StyleChips()
    {
        foreach (var (type, chip) in _chips)
        {
            var selected = type == _selectedMealType;
            chip.BackgroundColor = selected ? ThemeService.GetColor("Primary") : ThemeService.GetColor("Surface");
            chip.TextColor = selected ? ThemeService.GetColor("OnPrimary") : ThemeService.GetColor("TextSecondary");
        }
    }

    // ================== FANER OG SØK ==================

    private void OnMealsTabClicked(object sender, EventArgs e) => SetActiveKind(LibraryItemKind.Meal);

    private void OnIngredientsTabClicked(object sender, EventArgs e) => SetActiveKind(LibraryItemKind.Ingredient);

    private async void OnScanTabClicked(object sender, EventArgs e) =>
        await Toast.Make(AppResources.ScanComingSoon).Show();

    private async void SetActiveKind(LibraryItemKind kind)
    {
        _activeKind = kind;
        UpdateTabStyles();
        await LoadItemsAsync();
    }

    private void UpdateTabStyles()
    {
        var active = ThemeService.GetColor("Primary");
        var inactive = ThemeService.GetColor("TextMuted");

        MealsTabButton.TextColor = _activeKind == LibraryItemKind.Meal ? active : inactive;
        IngredientsTabButton.TextColor = _activeKind == LibraryItemKind.Ingredient ? active : inactive;
        ScanTabButton.TextColor = inactive;

        NewItemButton.Text = _activeKind == LibraryItemKind.Meal
            ? $"+ {AppResources.NewMeal}"
            : $"+ {AppResources.NewIngredient}";
        EmptyHintLabel.Text = string.Format(AppResources.CreateItemHint, NewItemButton.Text);
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    /// <summary>
    /// Opprett ny vare uten å forlate logge-flyten: editoren pushes oppå,
    /// søketeksten følger med som forhåndsutfylt navn, og ved lagring
    /// forhåndsvelges den nye varen her (se TrySelectCreatedItem).
    /// </summary>
    private async void OnNewItemClicked(object sender, EventArgs e)
    {
        var search = SearchEntry.Text?.Trim();
        var initialName = string.IsNullOrEmpty(search) ? null : search;

        if (_activeKind == LibraryItemKind.Meal)
            await Navigation.PushAsync(new MealBuilderPage(
                initialName: initialName,
                onSaved: meal => _createdId = meal.Id));
        else
            await Navigation.PushAsync(new IngredientEditorPage(
                initialName: initialName,
                onSaved: ingredient => _createdId = ingredient.Id));
    }

    /// <summary>
    /// Laster aktiv fane fra databasen — søket filtrerer deretter i minnet.
    /// </summary>
    private async Task LoadItemsAsync()
    {
        _loadedItems = _activeKind == LibraryItemKind.Meal
            ? (await LibraryStore.GetMealsAsync()).Select(LibraryItem.From).ToList()
            : (await LibraryStore.GetIngredientsAsync()).Select(LibraryItem.From).ToList();

        ApplyFilter();
        TrySelectCreatedItem();
        TrySelectCustomizedMeal();
    }

    /// <summary>
    /// Forhåndsvelger den tilpassede kopien etter retur fra Tilpass-modus.
    /// Kopien finnes ikke i bibliotek-listen — panelet fylles direkte.
    /// </summary>
    private void TrySelectCustomizedMeal()
    {
        if (_customizedMeal is not { } meal)
            return;

        _customizedMeal = null;
        ItemsList.SelectedItem = null;

        SelectItem(LibraryItem.From(meal));
        SelectedNameLabel.Text = $"{meal.Name} ({AppResources.CustomizedLabel})";
    }

    /// <summary>
    /// Forhåndsvelger en vare som nettopp ble opprettet fra denne flyten
    /// (retur fra editoren) — måltidstypen står urørt siden siden lå på stacken.
    /// </summary>
    private void TrySelectCreatedItem()
    {
        if (_createdId is not { } id)
            return;

        _createdId = null;

        // Nullstill søket så den nye varen garantert er synlig i listen
        if (!string.IsNullOrEmpty(SearchEntry.Text))
            SearchEntry.Text = string.Empty;

        var created = _loadedItems.FirstOrDefault(i => i.Source switch
        {
            LibraryIngredient ingredient => ingredient.Id == id,
            LibraryMeal meal => meal.Id == id,
            _ => false
        });

        if (created == null)
            return;

        ItemsList.SelectedItem = created;
        SelectItem(created);
    }

    private void ApplyFilter()
    {
        var search = SearchEntry.Text?.Trim() ?? string.Empty;

        ItemsList.ItemsSource = _loadedItems
            .Where(i => search.Length == 0
                        || i.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || i.Brand?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
            .OrderBy(i => i.Name)
            .ToList();
    }

    // ================== VALGT VARE ==================

    private void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is LibraryItem item)
            SelectItem(item);
    }

    private void SelectItem(LibraryItem item)
    {
        _selectedItem = item;
        SelectedNameLabel.Text = item.Brand == null ? item.Name : $"{item.Name} ({item.Brand})";
        AmountEntry.Text = item.DefaultAmountGrams.ToString("0.#", CultureInfo.CurrentCulture);

        BuildUnitButtons(item);
        CustomizeButton.IsVisible = item.Source is LibraryMeal;
        SelectionPanel.IsVisible = true;
        UpdateKcalPreview();
    }

    /// <summary>
    /// Åpner måltidet i byggeren som engangs-kopi: endringene logges,
    /// men malen i biblioteket røres ikke (med mindre brukeren
    /// eksplisitt velger "oppdater malen" der inne).
    /// </summary>
    private async void OnCustomizeClicked(object sender, EventArgs e)
    {
        if (_selectedItem?.Source is not LibraryMeal meal)
            return;

        await Navigation.PushAsync(new MealBuilderPage(meal,
            onCustomized: customized => _customizedMeal = customized));
    }

    /// <summary>
    /// Stykk-snarveier for varer med stykkvekt: "1 stk" setter gram-feltet
    /// til 1 x stykkvekten osv. Alt lagres fortsatt i gram.
    /// </summary>
    private void BuildUnitButtons(LibraryItem item)
    {
        UnitButtonsContainer.Children.Clear();

        if (item.UnitWeightGrams is not { } unitWeight || item.UnitName == null)
            return;

        for (var count = 1; count <= 2; count++)
        {
            var grams = unitWeight * count;
            var button = new Button
            {
                Text = $"{count} {item.UnitName}",
                FontSize = 12,
                Padding = new Thickness(10, 4),
                CornerRadius = 12,
                BackgroundColor = ThemeService.GetColor("Background"),
                TextColor = ThemeService.GetColor("TextSecondary")
            };
            button.Clicked += (_, _) =>
                AmountEntry.Text = grams.ToString("0.#", CultureInfo.CurrentCulture);

            UnitButtonsContainer.Children.Add(button);
        }
    }

    private void OnAmountChanged(object sender, TextChangedEventArgs e) => UpdateKcalPreview();

    private decimal ParseAmount()
    {
        var text = AmountEntry.Text?.Replace(',', '.') ?? string.Empty;
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var grams) && grams > 0
            ? grams
            : 0;
    }

    private void UpdateKcalPreview()
    {
        var amount = ParseAmount();
        KcalPreviewLabel.Text = _selectedItem == null ? string.Empty : $"{_selectedItem.CaloriesFor(amount)} kcal";
    }

    // ================== LAGRE ==================

    private async void OnConfirmAddClicked(object sender, EventArgs e)
    {
        var amount = ParseAmount();
        if (_selectedItem == null || amount <= 0)
            return;

        await DayLogStore.AddItemAsync(_selectedMealType, _selectedItem, amount);

        await AppToast.SuccessAsync(AppResources.ItemLogged);
        await Navigation.PopAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
