using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Calorie.Core.Features.Goals.ViewModels;

/// <summary>
/// Én budsjettrad i mål-oppsettet: måltidstype + valgfritt kcal-budsjett.
/// Tomt felt = ikke noe budsjett.
/// </summary>
public partial class BudgetOption(MealType value, string label) : ObservableObject
{
    public MealType Value { get; } = value;

    public string Label { get; } = label;

    [ObservableProperty]
    private string _amountText = string.Empty;
}
