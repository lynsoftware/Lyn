using Lyn.Backend.Apps.Calorie.Enums;

namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Brukerens dagsmål — kaloribudsjett og valgfrie makromål. Datert historikk:
/// én rad per endring, aldri oppdatering av eksisterende rader. Dashboardet
/// slår opp gjeldende mål per dato (siste rad med EffectiveFromDate &lt;= dagen),
/// slik at gamle dager måles mot målet som gjaldt DA.
/// </summary>
public class UserGoal
{
    // ================== PRIMARY KEY ==================
    public Guid Id { get; set; }

    // ================== FOREIGN KEYS ==================

    // Eier — AppUser.Id (Identity, string) fra Platform/Auth
    public string UserId { get; set; } = string.Empty;

    // ================== METADATA ==================

    public int DailyCalories { get; set; }

    // Maksgrense eller mål å nå — styrer hvordan dashboard/varsler tolker tallet
    public GoalMode Mode { get; set; } = GoalMode.Limit;

    // Valgfrie makromål i gram per dag
    public int? ProteinGrams { get; set; }
    public int? CarbsGrams { get; set; }
    public int? FatGrams { get; set; }

    // Første dag målet gjelder for (lokal dato, samme akse som LogEntry.LoggedDate)
    public DateOnly EffectiveFromDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Til last-write-wins ved synk — samme-dags-endringer oppdaterer raden
    public DateTime? UpdatedAtUtc { get; set; }

    // ================== NAVIGATION PROPERTIES ==================

    // Valgfrie budsjetter per måltidstype ("maks 500 kcal på middag")
    public ICollection<MealBudget> MealBudgets { get; set; } = new List<MealBudget>();
}
