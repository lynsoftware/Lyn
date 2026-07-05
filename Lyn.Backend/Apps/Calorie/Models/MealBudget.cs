using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Valgfritt kaloribudsjett per måltidstype under et UserGoal ("maks 500 kcal
/// på middag"). Maks én rad per måltidstype per mål. Henger på UserGoal og
/// arver dermed den daterte historikken — ny målperiode får sitt eget sett
/// budsjetter. Brukere uten budsjetter styres kun av dagstotalen.
/// </summary>
public class MealBudget
{
    // ================== PRIMARY KEY ==================
    public Guid Id { get; set; }

    // ================== FOREIGN KEYS ==================
    public Guid UserGoalId { get; set; }

    // ================== METADATA ==================
    public MealType MealType { get; set; }

    public int MaxCalories { get; set; }

    // ================== NAVIGATION PROPERTIES ==================
    public UserGoal UserGoal { get; set; } = null!;
}
