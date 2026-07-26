using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.Goals.Models;

/// <summary>
/// Valgfritt kaloribudsjett per måltidstype under et mål — speiler
/// MealBudget i backend. Maks én rad per måltidstype per mål.
/// </summary>
public class MealBudgetRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserGoalId { get; set; }

    public MealType MealType { get; set; }
    public int MaxCalories { get; set; }
}
