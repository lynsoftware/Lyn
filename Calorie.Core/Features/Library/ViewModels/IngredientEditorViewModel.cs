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
/// ViewModel for ingrediens-editoren: ny eller eksisterende ingrediens.
/// Ved lagring sendes id-en tilbake som Shell-resultat (CreatedItemId) —
/// logge-flyten bruker den til å forhåndsvelge varen.
/// </summary>
public partial class IngredientEditorViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IToastService _toast;
    private readonly ILibraryStore _library;

    private LibraryIngredient? _existing;

    [ObservableProperty]
    private string _title = AppResources.NewIngredient;

    // Sletting gjelder kun eksisterende ingredienser
    [ObservableProperty]
    private bool _isDeleteVisible;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _brand = string.Empty;

    [ObservableProperty]
    private string _caloriesText = string.Empty;

    [ObservableProperty]
    private string _proteinText = string.Empty;

    [ObservableProperty]
    private string _carbsText = string.Empty;

    [ObservableProperty]
    private string _fatText = string.Empty;

    [ObservableProperty]
    private string _unitNameText = string.Empty;

    [ObservableProperty]
    private string _unitWeightText = string.Empty;

    public IngredientEditorViewModel(INavigationService navigation, IToastService toast, ILibraryStore libraryStore)
    {
        _navigation = navigation;
        _toast = toast;
        _library = libraryStore;
    }

    // ===== Mottak av Shell-parametere — kalles av sidens ApplyQueryAttributes =====

    public void SetInitialName(string name)
    {
        if (_existing == null)
            Name = name;
    }

    public void LoadExisting(LibraryIngredient ingredient)
    {
        _existing = ingredient;

        IsDeleteVisible = true;
        Title = AppResources.TabIngredients;
        Name = ingredient.Name;
        Brand = ingredient.Brand ?? string.Empty;
        CaloriesText = FormatDecimal(ingredient.CaloriesPer100g);
        ProteinText = FormatDecimal(ingredient.ProteinPer100g);
        CarbsText = FormatDecimal(ingredient.CarbsPer100g);
        FatText = FormatDecimal(ingredient.FatPer100g);
        UnitNameText = ingredient.UnitName ?? string.Empty;
        UnitWeightText = ingredient.UnitWeightGrams is { } weight ? FormatDecimal(weight) : string.Empty;
    }

    // ===== Commands =====

    [RelayCommand]
    private async Task SaveAsync()
    {
        var name = Name.Trim();
        if (name.Length == 0)
            return;

        var target = _existing ?? new LibraryIngredient();

        target.Name = name;
        target.Brand = string.IsNullOrWhiteSpace(Brand) ? null : Brand.Trim();
        target.CaloriesPer100g = ParseDecimal(CaloriesText);
        target.ProteinPer100g = ParseDecimal(ProteinText);
        target.CarbsPer100g = ParseDecimal(CarbsText);
        target.FatPer100g = ParseDecimal(FatText);

        // Stykk-støtte er valgfri — krever både navn og vekt for å gjelde
        var unitName = UnitNameText.Trim();
        var unitWeight = ParseDecimal(UnitWeightText);
        target.UnitName = unitName.Length == 0 || unitWeight <= 0 ? null : unitName;
        target.UnitWeightGrams = target.UnitName == null ? null : unitWeight;

        await _library.SaveIngredientAsync(target);

        await _toast.ShowSuccessAsync(AppResources.IngredientSaved);
        await _navigation.GoBackAsync(NavKeys.CreatedItemId, target.Id);
    }

    /// <summary>
    /// Sletter ingrediensen — blokkeres med forklaring hvis den brukes i et
    /// måltid (Restrict-regelen; storen svarer false og rører ingenting).
    /// </summary>
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_existing is not { } ingredient)
            return;

        if (!await _library.DeleteIngredientAsync(ingredient.Id))
        {
            await _toast.ShowErrorAsync(AppResources.IngredientInUse);
            return;
        }

        await _toast.ShowSuccessAsync(AppResources.IngredientDeleted);
        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CloseAsync() => _navigation.GoBackAsync();

    private static string FormatDecimal(decimal value) => value.ToString("0.##", CultureInfo.CurrentCulture);

    private static decimal ParseDecimal(string text)
    {
        var normalized = text.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value >= 0
            ? value
            : 0;
    }
}
