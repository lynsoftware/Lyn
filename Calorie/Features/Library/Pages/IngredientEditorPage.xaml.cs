using System.Globalization;
using Calorie.Core.Features.Library;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.Library.Pages;

/// <summary>
/// Editor for ny eller eksisterende ingrediens. Redigering muterer
/// bibliotek-objektet direkte (in-memory i Fase 4A).
/// </summary>
public partial class IngredientEditorPage : ContentPage
{
    private readonly LibraryIngredient? _existing;
    private readonly Action<LibraryIngredient>? _onSaved;

    /// <param name="initialName">Forhåndsutfylt navn ved ny ingrediens
    /// (f.eks. søketeksten fra logge-flyten).</param>
    /// <param name="onSaved">Kalles med den lagrede ingrediensen — lar
    /// logge-flyten forhåndsvelge den nye varen ved retur.</param>
    public IngredientEditorPage(LibraryIngredient? existing = null,
        string? initialName = null,
        Action<LibraryIngredient>? onSaved = null)
    {
        InitializeComponent();
        _existing = existing;
        _onSaved = onSaved;

        TitleLabel.Text = existing == null ? AppResources.NewIngredient : AppResources.TabIngredients;

        if (existing == null)
        {
            NameEntry.Text = initialName;
        }
        else
        {
            NameEntry.Text = existing.Name;
            BrandEntry.Text = existing.Brand;
            CaloriesEntry.Text = FormatDecimal(existing.CaloriesPer100g);
            ProteinEntry.Text = FormatDecimal(existing.ProteinPer100g);
            CarbsEntry.Text = FormatDecimal(existing.CarbsPer100g);
            FatEntry.Text = FormatDecimal(existing.FatPer100g);
            UnitNameEntry.Text = existing.UnitName;
            UnitWeightEntry.Text = existing.UnitWeightGrams is { } weight ? FormatDecimal(weight) : null;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        var target = _existing ?? new LibraryIngredient();

        target.Name = name;
        target.Brand = string.IsNullOrWhiteSpace(BrandEntry.Text) ? null : BrandEntry.Text.Trim();
        target.CaloriesPer100g = ParseDecimal(CaloriesEntry.Text);
        target.ProteinPer100g = ParseDecimal(ProteinEntry.Text);
        target.CarbsPer100g = ParseDecimal(CarbsEntry.Text);
        target.FatPer100g = ParseDecimal(FatEntry.Text);

        // Stykk-støtte er valgfri — krever både navn og vekt for å gjelde
        var unitName = UnitNameEntry.Text?.Trim();
        var unitWeight = ParseDecimal(UnitWeightEntry.Text);
        target.UnitName = string.IsNullOrEmpty(unitName) || unitWeight <= 0 ? null : unitName;
        target.UnitWeightGrams = target.UnitName == null ? null : unitWeight;

        await LibraryStore.SaveIngredientAsync(target);
        _onSaved?.Invoke(target);

        await AppToast.SuccessAsync(AppResources.IngredientSaved);
        await Navigation.PopAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private static string FormatDecimal(decimal value) => value.ToString("0.##", CultureInfo.CurrentCulture);

    private static decimal ParseDecimal(string? text)
    {
        var normalized = text?.Replace(',', '.') ?? string.Empty;
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value >= 0
            ? value
            : 0;
    }
}
