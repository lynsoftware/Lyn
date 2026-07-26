using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.DailyLog.Models;

/// <summary>
/// Én måltidsseksjon i dagsloggen: budsjett (valgfritt) + loggførte rader.
/// </summary>
public class MealSection
{
    public MealType MealType { get; set; }

    // Fra MealBudget — null hvis brukeren ikke har satt budsjett for typen
    public int? MaxCalories { get; set; }

    public List<LoggedItem> Items { get; set; } = new();

    public int EatenCalories => Items.Sum(i => i.Calories);

    // Null når det ikke finnes budsjett — da vises kun "spist"
    public int? RemainingCalories => MaxCalories.HasValue ? MaxCalories.Value - EatenCalories : null;
}
