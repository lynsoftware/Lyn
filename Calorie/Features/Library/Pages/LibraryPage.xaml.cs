using Calorie.Features.DailyLog.Pages;
using Calorie.Features.Settings.Pages;
using Calorie.Features.Stats.Pages;
using Calorie.Core.Features.Library;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Biblioteket: brukerens måltider og ingredienser. Trykk på et innslag
/// åpner riktig editor; ny-knappen følger aktiv fane.
/// </summary>
public partial class LibraryPage : ContentPage
{
    private LibraryItemKind _activeKind = LibraryItemKind.Meal;
    private List<LibraryItem> _loadedItems = [];

    public LibraryPage()
    {
        InitializeComponent();
        UpdateTabStyles();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadItemsAsync();
    }

    // ================== FANER OG SØK ==================

    private void OnMealsTabClicked(object sender, EventArgs e) => SetActiveKind(LibraryItemKind.Meal);

    private void OnIngredientsTabClicked(object sender, EventArgs e) => SetActiveKind(LibraryItemKind.Ingredient);

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

        NewItemButton.Text = _activeKind == LibraryItemKind.Meal
            ? $"+ {AppResources.NewMeal}"
            : $"+ {AppResources.NewIngredient}";
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    /// <summary>
    /// Laster aktiv fane fra databasen — søket filtrerer deretter i minnet.
    /// </summary>
    private async Task LoadItemsAsync()
    {
        _loadedItems = _activeKind == LibraryItemKind.Meal
            ? (await LibraryStore.GetMealsAsync()).Select(LibraryItem.From).ToList()
            : (await LibraryStore.GetIngredientsAsync()).Select(LibraryItem.From).ToList();

        ApplyFilter();
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

    // ================== OPPRETT OG REDIGER ==================

    private async void OnNewItemClicked(object sender, EventArgs e)
    {
        if (_activeKind == LibraryItemKind.Meal)
            await Navigation.PushAsync(new MealBuilderPage());
        else
            await Navigation.PushAsync(new IngredientEditorPage());
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not LibraryItem item)
            return;

        // Nullstill så samme rad kan trykkes igjen etter tilbake-navigering
        ItemsList.SelectedItem = null;

        switch (item.Source)
        {
            case LibraryMeal meal:
                await Navigation.PushAsync(new MealBuilderPage(meal));
                break;
            case LibraryIngredient ingredient:
                await Navigation.PushAsync(new IngredientEditorPage(ingredient));
                break;
        }
    }

    // ================== NAVBAR ==================

    private async void OnHomeClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnLogClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AddLogEntryPage());

    private async void OnStatsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new StatsPage());

    private async void OnSettingsClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SettingsPage());
}
