using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

/// <summary>
/// Sync av budgetet til en bruker
/// </summary>
public class SyncMealBudgetDto
{
    public Guid Id { get; set; }
    public MealType MealType { get; set; }
    public int MaxCalories { get; set; }
}