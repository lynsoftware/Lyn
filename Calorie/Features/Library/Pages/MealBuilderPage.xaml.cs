using System.Globalization;
using Calorie.Core.Features.Library;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Måltidsbygger: komponer et måltid av ingredienser med gram, se totalen
/// live. Jobber på en arbeidskopi av komponentlisten — skrives tilbake
/// først ved lagring.
/// </summary>
public partial class MealBuilderPage : ContentPage
{
    private readonly LibraryMeal? _existing;
    private readonly Action<LibraryMeal>? _onSaved;
    private readonly Action<LibraryMeal>? _onCustomized;
    private readonly List<MealComponent> _components;

    // Id-en til en ingrediens opprettet fra denne byggeren — forhåndsvelges i pickeren ved retur
    private Guid? _createdIngredientId;

    // Stykk-modus: mengdefeltet tolkes som antall stykk i stedet for gram
    private bool _useUnits;

    /// <param name="initialName">Forhåndsutfylt navn ved nytt måltid
    /// (f.eks. søketeksten fra logge-flyten).</param>
    /// <param name="onSaved">Kalles med det lagrede måltidet — lar
    /// logge-flyten forhåndsvelge den nye varen ved retur.</param>
    /// <param name="onCustomized">Tilpass-modus: kalles med en engangs-kopi
    /// av måltidet (lagres IKKE i biblioteket) — malen oppdateres kun via
    /// den egne "oppdater malen"-knappen.</param>
    public MealBuilderPage(LibraryMeal? existing = null,
        string? initialName = null,
        Action<LibraryMeal>? onSaved = null,
        Action<LibraryMeal>? onCustomized = null)
    {
        InitializeComponent();
        _existing = existing;
        _onSaved = onSaved;
        _onCustomized = onCustomized;

        TitleLabel.Text = existing == null ? AppResources.NewMeal : AppResources.TabMeals;
        NameEntry.Text = existing?.Name ?? initialName;

        if (onCustomized != null)
        {
            TitleLabel.Text = AppResources.CustomizeMeal;
            SaveButton.Text = AppResources.UseInLog;
            UpdateTemplateButton.Text = AppResources.UpdateTemplateToo;
            UpdateTemplateButton.IsVisible = existing != null;
        }

        // Arbeidskopi — avbryt (X) skal ikke etterlate halvferdige endringer
        _components = existing?.Components.Select(c => new MealComponent
        {
            Ingredient = c.Ingredient,
            AmountGrams = c.AmountGrams
        }).ToList() ?? [];

        RebuildComponentRows();
        UpdateUnitToggle();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Last på nytt ved retur fra ingrediens-editoren så den nye varen finnes i pickeren
        if (IngredientPicker.ItemsSource == null || _createdIngredientId != null)
            await ReloadIngredientPickerAsync();
    }

    private async Task ReloadIngredientPickerAsync()
    {
        var ingredients = await LibraryStore.GetIngredientsAsync();
        IngredientPicker.ItemsSource = ingredients;

        if (_createdIngredientId is not { } id)
            return;

        _createdIngredientId = null;
        IngredientPicker.SelectedItem = ingredients.FirstOrDefault(i => i.Id == id);
    }

    /// <summary>
    /// Opprett ny ingrediens uten å forlate måltidsbyggeren: editoren pushes
    /// oppå, og ved lagring forhåndsvelges den nye i pickeren (arbeidskopien
    /// av komponentlisten står urørt siden siden lå på stacken).
    /// </summary>
    private async void OnNewIngredientClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new IngredientEditorPage(
            onSaved: ingredient => _createdIngredientId = ingredient.Id));

    // ================== ENHETS-TOGGLE (gram/stykk) ==================

    private LibraryIngredient? SelectedIngredient => IngredientPicker.SelectedItem as LibraryIngredient;

    private bool SelectedHasUnit => SelectedIngredient is { UnitWeightGrams: > 0, UnitName: not null };

    private void OnIngredientPicked(object sender, EventArgs e) => UpdateUnitToggle();

    private void OnUnitToggleClicked(object sender, EventArgs e)
    {
        if (!SelectedHasUnit)
            return;

        _useUnits = !_useUnits;
        UpdateUnitToggle();
    }

    private void UpdateUnitToggle()
    {
        if (!SelectedHasUnit)
            _useUnits = false;

        UnitToggleButton.IsEnabled = SelectedHasUnit;
        UnitToggleButton.Opacity = SelectedHasUnit ? 1 : 0.5;
        UnitToggleButton.Text = _useUnits ? SelectedIngredient!.UnitName : "g";
        AmountEntry.Placeholder = _useUnits ? "1" : "100";
    }

    // ================== KOMPONENTER ==================

    private void OnAddComponentClicked(object sender, EventArgs e)
    {
        if (SelectedIngredient is not { } ingredient)
            return;

        var amount = ParseDecimal(AmountEntry.Text);

        // Stykk-modus: antall stykk x stykkvekt — lagres alltid i gram
        var grams = _useUnits && ingredient.UnitWeightGrams is { } unitWeight
            ? (amount > 0 ? amount : 1) * unitWeight
            : amount > 0 ? amount : ingredient.UnitWeightGrams ?? 100;

        _components.Add(new MealComponent { Ingredient = ingredient, AmountGrams = grams });

        AmountEntry.Text = null;
        RebuildComponentRows();
    }

    private void RebuildComponentRows()
    {
        ComponentsContainer.Children.Clear();

        foreach (var component in _components)
            ComponentsContainer.Children.Add(CreateComponentRow(component));

        UpdateTotal();
    }

    /// <summary>
    /// Én komponentrad: navn + gram til venstre, kcal og fjern-knapp til høyre.
    /// </summary>
    private Grid CreateComponentRow(MealComponent component)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            ]
        };

        var nameStack = new VerticalStackLayout { Spacing = 0 };
        nameStack.Children.Add(new Label
        {
            Text = component.Ingredient.Name,
            FontSize = 15,
            TextColor = ThemeService.GetColor("TextPrimary")
        });
        nameStack.Children.Add(new Label
        {
            Text = FormatAmount(component),
            FontSize = 12,
            TextColor = ThemeService.GetColor("TextMuted")
        });
        row.Add(nameStack, 0);

        row.Add(new Label
        {
            Text = $"{component.Calories} kcal",
            FontSize = 14,
            TextColor = ThemeService.GetColor("TextSecondary"),
            VerticalOptions = LayoutOptions.Center
        }, 1);

        var removeButton = new Button
        {
            Text = "\uf00d",
            FontFamily = "FontAwesome",
            FontSize = 14,
            TextColor = ThemeService.GetColor("TextMuted"),
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(6, 0)
        };
        removeButton.Clicked += (_, _) =>
        {
            _components.Remove(component);
            RebuildComponentRows();
        };
        row.Add(removeButton, 2);

        return row;
    }

    /// <summary>
    /// "2 stk · 120 g" når mengden går opp i hele stykk — ellers bare gram.
    /// </summary>
    private static string FormatAmount(MealComponent component)
    {
        var gramsText = $"{component.AmountGrams:0.#} g";

        if (component.Ingredient.UnitWeightGrams is not { } unitWeight || unitWeight <= 0
            || component.Ingredient.UnitName is not { } unitName)
            return gramsText;

        var units = component.AmountGrams / unitWeight;
        return units == Math.Floor(units)
            ? $"{units:0.#} {unitName} · {gramsText}"
            : gramsText;
    }

    private void UpdateTotal()
    {
        var totalGrams = _components.Sum(c => c.AmountGrams);
        var totalCalories = _components.Sum(c => c.Calories);
        TotalLabel.Text = $"{AppResources.TotalLabel}: {totalCalories} kcal · {totalGrams:0.#} g";
    }

    // ================== LAGRE ==================

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name) || _components.Count == 0)
            return;

        // Tilpass-modus: engangs-kopi rett til logge-flyten — malen røres ikke
        if (_onCustomized != null)
        {
            _onCustomized(new LibraryMeal
            {
                Name = name,
                Brand = _existing?.Brand,
                Components = _components
            });
            await Navigation.PopAsync();
            return;
        }

        var target = _existing ?? new LibraryMeal();
        target.Name = name;
        target.Components = _components;

        await LibraryStore.SaveMealAsync(target);
        _onSaved?.Invoke(target);

        await AppToast.SuccessAsync(AppResources.MealSaved);
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Tilpass-modus: skriv endringene tilbake til malen i biblioteket OG
    /// bruk resultatet i loggen — brukerens eksplisitte valg.
    /// </summary>
    private async void OnUpdateTemplateClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name) || _components.Count == 0 || _existing == null)
            return;

        _existing.Name = name;
        _existing.Components = _components;

        await LibraryStore.SaveMealAsync(_existing);
        _onCustomized?.Invoke(_existing);

        await AppToast.SuccessAsync(AppResources.MealSaved);
        await Navigation.PopAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private static decimal ParseDecimal(string? text)
    {
        var normalized = text?.Replace(',', '.') ?? string.Empty;
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : 0;
    }
}
