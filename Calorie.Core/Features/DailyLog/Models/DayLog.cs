using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.DailyLog.Models;

/// <summary>
/// Visningsmodell for én dag i dagsloggen: mål, makroer og måltidsseksjoner.
/// </summary>
public class DayLog
{
    public DateOnly Date { get; set; }

    public int GoalCalories { get; set; }
    public GoalMode Mode { get; set; }

    // Spiste makroer i gram (sum for dagen)
    public int ProteinGrams { get; set; }
    public int CarbsGrams { get; set; }
    public int FatGrams { get; set; }

    // Valgfrie makromål fra GoalSettings — null skjuler målvisningen
    public int? ProteinGoalGrams { get; set; }
    public int? CarbsGoalGrams { get; set; }
    public int? FatGoalGrams { get; set; }

    // Valgfri kos-andel av dagsmålet i prosent (null = featuren av)
    public int? TreatPercent { get; set; }

    public List<MealSection> Sections { get; set; } = new();

    public int TotalEatenCalories => Sections.Sum(s => s.EatenCalories);

    public int RemainingCalories => GoalCalories - TotalEatenCalories;

    // ===== Kos-regnskap — kos teller i dagstotalen, men vises separat =====

    public int TreatCalories =>
        Sections.FirstOrDefault(s => s.MealType == MealType.Treat)?.EatenCalories ?? 0;

    // Kos-budsjettet i kcal, avledet av prosenten — skalerer med dagsmålet
    public int? TreatAllowanceCalories =>
        TreatPercent is { } percent ? GoalCalories * percent / 100 : null;

    public int? TreatRemainingCalories =>
        TreatAllowanceCalories is { } allowance ? allowance - TreatCalories : null;
}
