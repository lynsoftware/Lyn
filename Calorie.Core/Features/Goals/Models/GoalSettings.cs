using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.Goals.Models;

/// <summary>
/// Brukerens mål: dagsbudsjett, tolkningsmodus, valgfrie makromål og
/// valgfrie budsjetter per måltidstype. Speiler UserGoal + MealBudget
/// i backend (uten datert historikk — den håndteres av lagringen i 4B).
/// </summary>
public class GoalSettings
{
    public int DailyCalories { get; set; } = 1500;

    public GoalMode Mode { get; set; } = GoalMode.Limit;

    // Valgfrie makromål i gram per dag
    public int? ProteinGoalGrams { get; set; }
    public int? CarbsGoalGrams { get; set; }
    public int? FatGoalGrams { get; set; }

    // Valgfri kos-andel av dagsmålet i prosent (null = av) — skalerer med dagsmålet
    public int? TreatPercent { get; set; }

    // Valgfritt maks-kcal per måltidstype — mangler typen, finnes ikke budsjett
    public Dictionary<MealType, int> MealBudgets { get; set; } = new();
}
