using Calorie.Core.Common;
namespace Calorie.Core.Features.DailyLog;

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

    public List<MealSection> Sections { get; set; } = new();

    public int TotalEatenCalories => Sections.Sum(s => s.EatenCalories);

    public int RemainingCalories => GoalCalories - TotalEatenCalories;
}
