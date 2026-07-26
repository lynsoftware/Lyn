using System.Globalization;
using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calorie.Core.Features.Library.ViewModels;

/// <summary>
/// ViewModel for måltidsbyggeren. Tre moduser styrt av Shell-parametere:
/// ny (evt. med forhåndsutfylt navn), rediger eksisterende (MealToEdit),
/// og Tilpass (MealToCustomize) — engangs-kopi som sendes tilbake som
/// resultat uten å røre malen, med egen knapp for å oppdatere malen også.
/// Jobber alltid på en arbeidskopi av komponentlisten.
/// </summary>
public partial class MealBuilderViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IToastService _toast;
    private readonly ILibraryStore _library;

    private LibraryMeal? _existing;
    private bool _customizeMode;
    private readonly List<MealComponent> _components = [];

    // Ingrediens opprettet fra byggeren — forhåndsvelges i pickeren ved retur
    private Guid? _pendingCreatedIngredientId;

    [ObservableProperty]
    private string _title = AppResources.NewMeal;

    [ObservableProperty]
    private string _saveButtonText = AppResources.Save;

    [ObservableProperty]
    private bool _isUpdateTemplateVisible;

    // Sletting gjelder kun redigering av eksisterende mal — ikke ny og ikke Tilpass
    [ObservableProperty]
    private bool _isDeleteVisible;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private List<LibraryIngredient> _ingredients = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitToggleText), nameof(IsUnitToggleEnabled), nameof(AmountPlaceholder))]
    private LibraryIngredient? _selectedIngredient;

    // Legg-til-kilde: enkelt-ingrediens eller et annet måltid (flates ut)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIngredientSource))]
    [NotifyCanExecuteChangedFor(nameof(NewIngredientCommand))]
    private bool _isMealSource;

    [ObservableProperty]
    private List<LibraryMeal> _meals = [];

    [ObservableProperty]
    private LibraryMeal? _selectedMeal;

    [ObservableProperty]
    private string _amountText = string.Empty;

    [ObservableProperty]
    private List<MealComponentRow> _componentRows = [];

    [ObservableProperty]
    private string _totalText = string.Empty;

    // Stykk-modus: mengdefeltet tolkes som antall stykk i stedet for gram
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitToggleText), nameof(AmountPlaceholder))]
    private bool _useUnits;

    // ===== Manuell modus (ferdigrett): næringsinnhold som tall i stedet for
    // ingredienser. Lagres som ingrediens + ett-komponents måltid — samme
    // modell som EAN/ReadyMeal-flyten (én beregningsvei, gram x per-100g). =====

    // Kun for nye måltider — redigering/Tilpass åpner alltid komponent-modus
    [ObservableProperty]
    private bool _canChooseMode = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComponentMode))]
    private bool _isManualMode;

    // Tolkningen av tallene: per 100 g (etikettens venstre kolonne) eller hele pakken
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPer100g), nameof(ManualTotalPreview))]
    private bool _isPerPackage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManualTotalPreview))]
    private string _packageWeightText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ManualTotalPreview))]
    private string _manualCaloriesText = string.Empty;

    [ObservableProperty]
    private string _manualProteinText = string.Empty;

    [ObservableProperty]
    private string _manualCarbsText = string.Empty;

    [ObservableProperty]
    private string _manualFatText = string.Empty;

    public bool IsComponentMode => !IsManualMode;

    public bool IsPer100g => !IsPerPackage;

    /// <summary>
    /// Live totalpreview i manuell modus: "Totalt: 428 kcal · 450 g".
    /// Per pakke = kcal-tallet direkte; per 100 g = skalert med vekten.
    /// </summary>
    public string ManualTotalPreview
    {
        get
        {
            var weight = ParseDecimal(PackageWeightText);
            var calories = ParseDecimal(ManualCaloriesText);
            if (weight <= 0 || calories <= 0)
                return string.Empty;

            var total = IsPerPackage ? calories : calories * weight / 100m;
            return $"{AppResources.TotalLabel}: {Math.Round(total)} kcal · {weight:0.#} g";
        }
    }

    [RelayCommand]
    private void SelectComponentMode() => IsManualMode = false;

    [RelayCommand]
    private void SelectManualMode() => IsManualMode = true;

    [RelayCommand]
    private void SelectPer100g() => IsPerPackage = false;

    [RelayCommand]
    private void SelectPerPackage() => IsPerPackage = true;

    public MealBuilderViewModel(INavigationService navigation, IToastService toast, ILibraryStore libraryStore)
    {
        _navigation = navigation;
        _toast = toast;
        _library = libraryStore;

        RebuildComponentRows();
    }

    // ===== Avledet tilstand for enhets-togglen =====

    private bool SelectedHasUnit => SelectedIngredient is { UnitWeightGrams: > 0, UnitName: not null };

    public bool IsUnitToggleEnabled => SelectedHasUnit;

    public string UnitToggleText => UseUnits && SelectedIngredient?.UnitName is { } unitName ? unitName : "g";

    public string AmountPlaceholder => UseUnits ? "1" : "100";

    public bool IsIngredientSource => !IsMealSource;

    partial void OnSelectedIngredientChanged(LibraryIngredient? value)
    {
        if (!SelectedHasUnit)
            UseUnits = false;
    }

    // Hele måltidet er vanligst — forhåndsfyll med kildens totalvekt
    partial void OnSelectedMealChanged(LibraryMeal? value)
    {
        if (value != null)
            AmountText = value.TotalGrams.ToString("0.#", CultureInfo.CurrentCulture);
    }

    partial void OnIsMealSourceChanged(bool value) => UseUnits = false;

    [RelayCommand]
    private void SelectIngredientSource() => IsMealSource = false;

    [RelayCommand]
    private void SelectMealSource() => IsMealSource = true;

    // ===== Mottak av Shell-parametere — kalles av sidens ApplyQueryAttributes =====

    public void SetInitialName(string name)
    {
        if (_existing == null)
            Name = name;
    }

    public void LoadExisting(LibraryMeal meal)
    {
        _existing = meal;
        Title = AppResources.TabMeals;
        Name = meal.Name;
        CanChooseMode = false;
        IsManualMode = false;
        IsDeleteVisible = true;

        CopyComponentsFrom(meal);
    }

    public void LoadForCustomize(LibraryMeal meal)
    {
        _existing = meal;
        _customizeMode = true;
        CanChooseMode = false;
        IsManualMode = false;

        Title = AppResources.CustomizeMeal;
        SaveButtonText = AppResources.UseInLog;
        IsUpdateTemplateVisible = true;
        Name = meal.Name;

        CopyComponentsFrom(meal);
    }

    public void NotifyIngredientCreated(Guid id) => _pendingCreatedIngredientId = id;

    // Arbeidskopi — avbryt (X) skal ikke etterlate halvferdige endringer
    private void CopyComponentsFrom(LibraryMeal meal)
    {
        _components.Clear();
        _components.AddRange(meal.Components.Select(c => new MealComponent
        {
            Ingredient = c.Ingredient,
            AmountGrams = c.AmountGrams
        }));

        RebuildComponentRows();
    }

    // ===== Ingredienspickeren =====

    /// <summary>
    /// Laster pickerne ved første visning og ved retur fra ingrediens-editoren
    /// (da forhåndsvelges den nyopprettede ingrediensen). Måltidet som
    /// redigeres holdes utenfor måltidslisten — ingen selvreferanse.
    /// </summary>
    [RelayCommand]
    private async Task LoadIngredientsAsync()
    {
        if (Ingredients.Count > 0 && _pendingCreatedIngredientId == null)
            return;

        Ingredients = await _library.GetIngredientsAsync();
        Meals = (await _library.GetMealsAsync())
            .Where(m => m.Id != _existing?.Id)
            .ToList();

        if (_pendingCreatedIngredientId is not { } id)
            return;

        _pendingCreatedIngredientId = null;
        SelectedIngredient = Ingredients.FirstOrDefault(i => i.Id == id);
    }

    // ===== Komponenter =====

    [RelayCommand]
    private void AddComponent()
    {
        if (IsMealSource)
        {
            AddMealComponents();
            return;
        }

        if (SelectedIngredient is not { } ingredient)
            return;

        var amount = ParseDecimal(AmountText);

        // Stykk-modus: antall stykk x stykkvekt — lagres alltid i gram
        var grams = UseUnits && ingredient.UnitWeightGrams is { } unitWeight
            ? (amount > 0 ? amount : 1) * unitWeight
            : amount > 0 ? amount : ingredient.UnitWeightGrams ?? 100;

        _components.Add(new MealComponent { Ingredient = ingredient, AmountGrams = grams });

        AmountText = string.Empty;
        RebuildComponentRows();
    }

    /// <summary>
    /// Flater ut et måltid: ingrediens-komponentene kopieres inn skalert til
    /// valgt mengde (faktor = valgt gram / kildens totalvekt). Ingen
    /// måltid-referanse i modellen — et sammensatt måltid er et
    /// øyeblikksbilde av oppskriften, og radene kan justeres enkeltvis.
    /// </summary>
    private void AddMealComponents()
    {
        if (SelectedMeal is not { } source || source.TotalGrams <= 0)
            return;

        var amount = ParseDecimal(AmountText);
        var grams = amount > 0 ? amount : source.TotalGrams;
        var factor = grams / source.TotalGrams;

        foreach (var component in source.Components)
        {
            _components.Add(new MealComponent
            {
                Ingredient = component.Ingredient,
                AmountGrams = component.AmountGrams * factor
            });
        }

        AmountText = string.Empty;
        RebuildComponentRows();
    }

    [RelayCommand]
    private void RemoveComponent(MealComponentRow row)
    {
        _components.Remove(row.Component);
        RebuildComponentRows();
    }

    [RelayCommand]
    private void ToggleUnit()
    {
        if (SelectedHasUnit)
            UseUnits = !UseUnits;
    }

    // Kun i ingrediens-modus — nye måltider lages ikke inne i et måltid
    [RelayCommand(CanExecute = nameof(IsIngredientSource))]
    private Task NewIngredientAsync() => _navigation.GoToIngredientEditorAsync();

    private void RebuildComponentRows()
    {
        ComponentRows = _components
            .Select(c => new MealComponentRow(c, c.Ingredient.Name, FormatAmount(c), $"{c.Calories} kcal"))
            .ToList();

        var totalGrams = _components.Sum(c => c.AmountGrams);
        var totalCalories = _components.Sum(c => c.Calories);
        TotalText = $"{AppResources.TotalLabel}: {totalCalories} kcal · {totalGrams:0.#} g";
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

    // ===== Lagre =====

    [RelayCommand]
    private async Task SaveAsync()
    {
        var name = Name.Trim();
        if (name.Length == 0)
            return;

        if (IsManualMode)
        {
            await SaveManualAsync(name);
            return;
        }

        if (_components.Count == 0)
            return;

        // Tilpass-modus: engangs-kopi rett tilbake til logge-flyten — malen røres ikke
        if (_customizeMode)
        {
            var transient = new LibraryMeal
            {
                Name = name,
                Brand = _existing?.Brand,
                Components = _components.ToList()
            };

            await _navigation.GoBackAsync(NavKeys.MealCustomized, transient);
            return;
        }

        var target = _existing ?? new LibraryMeal();
        target.Name = name;
        target.Components = _components.ToList();

        await _library.SaveMealAsync(target);

        await _toast.ShowSuccessAsync(AppResources.MealSaved);
        await _navigation.GoBackAsync(NavKeys.CreatedItemId, target.Id);
    }

    /// <summary>
    /// Manuell modus: etikettverdiene lagres som en ingrediens med pakke-
    /// stykkvekt, innpakket i et ett-komponents måltid (ReadyMeal-mønsteret).
    /// Per pakke-tall regnes om til per 100 g — modellen kjenner kun én vei.
    /// </summary>
    private async Task SaveManualAsync(string name)
    {
        var weight = ParseDecimal(PackageWeightText);
        if (weight <= 0)
            return;

        var calories = ParseDecimal(ManualCaloriesText);
        if (calories <= 0)
            return;

        var factor = IsPerPackage ? 100m / weight : 1m;

        var ingredient = new LibraryIngredient
        {
            Name = name,
            CaloriesPer100g = calories * factor,
            ProteinPer100g = ParseDecimal(ManualProteinText) * factor,
            CarbsPer100g = ParseDecimal(ManualCarbsText) * factor,
            FatPer100g = ParseDecimal(ManualFatText) * factor,
            UnitName = AppResources.PackageUnitName,
            UnitWeightGrams = weight
        };

        await _library.SaveIngredientAsync(ingredient);

        var meal = new LibraryMeal
        {
            Name = name,
            Components = [new MealComponent { Ingredient = ingredient, AmountGrams = weight }]
        };

        await _library.SaveMealAsync(meal);

        await _toast.ShowSuccessAsync(AppResources.MealSaved);
        await _navigation.GoBackAsync(NavKeys.CreatedItemId, meal.Id);
    }

    /// <summary>
    /// Tilpass-modus: skriv endringene tilbake til malen i biblioteket OG
    /// bruk resultatet i loggen — brukerens eksplisitte valg.
    /// </summary>
    [RelayCommand]
    private async Task UpdateTemplateAsync()
    {
        var name = Name.Trim();
        if (name.Length == 0 || _components.Count == 0 || _existing == null)
            return;

        _existing.Name = name;
        _existing.Components = _components.ToList();

        await _library.SaveMealAsync(_existing);

        await _toast.ShowSuccessAsync(AppResources.MealSaved);
        await _navigation.GoBackAsync(NavKeys.MealCustomized, _existing);
    }

    /// <summary>Sletter malen — alltid lov, loggen er snapshot og rammes ikke.</summary>
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existing is not { } meal)
            return;

        await _library.DeleteMealAsync(meal.Id);

        await _toast.ShowSuccessAsync(AppResources.MealDeleted);
        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();

    private static decimal ParseDecimal(string text)
    {
        var normalized = text.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : 0;
    }
}
