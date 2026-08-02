using Lyn.Backend.Apps.Calorie.Models;

namespace Lyn.Backend.Apps.Calorie.Sync.Repositories;

/// <summary>
/// Sync-slicens dataadgang — det eneste laget som ser CalorieDbContext.
/// Ett repo for hele slicen (repo per use case, ikke per entitet): pushen
/// anvendes som ÉN batch med SaveChangesAsync som eneste commit-punkt.
/// Hent-metodene for push sporer endringer (servicen muterer entitetene);
/// GetAll-metodene for state er ren lesing uten tracking.
/// </summary>
public interface ISyncRepository
{
    // ===== State (GET /state) — ren lesing =====

    /// <summary>Alle brukerens ingredienser — ren lesing, uten tracking.</summary>
    Task<List<Ingredient>> GetAllIngredientsAsync(string userId, CancellationToken ct = default);

    /// <summary>Alle brukerens måltider med komponentene (Ingredients) — ren lesing.</summary>
    Task<List<Meal>> GetAllMealsAsync(string userId, CancellationToken ct = default);

    /// <summary>Alle brukerens loggrader med snapshot-radene (Ingredients) — ren lesing.</summary>
    Task<List<LogEntry>> GetAllLogEntriesAsync(string userId, CancellationToken ct = default);

    /// <summary>Hele målhistorikken med budsjettene (MealBudgets) — ren lesing.</summary>
    Task<List<UserGoal>> GetAllGoalsAsync(string userId, CancellationToken ct = default);

    // ===== Push (POST /push) — hent eksisterende for LWW-sammenligning =====
    // Sporede entiteter: servicen muterer dem, SaveChangesAsync committer.
    // Alltid filtrert på userId — en annen brukers Guid gir bare "finnes ikke".

    /// <summary>Brukerens eksisterende ingredienser blant id-ene — for LWW-oppslag.</summary>
    Task<Dictionary<Guid, Ingredient>> GetIngredientsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Brukerens eksisterende måltider (med komponenter) blant id-ene — for LWW-oppslag.</summary>
    Task<Dictionary<Guid, Meal>> GetMealsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Brukerens eksisterende loggrader (med snapshot-rader) blant id-ene — for LWW-oppslag.</summary>
    Task<Dictionary<Guid, LogEntry>> GetLogEntriesByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Brukerens eksisterende mål (med budsjetter) blant id-ene — for LWW-oppslag.</summary>
    Task<Dictionary<Guid, UserGoal>> GetGoalsByIdsAsync(
        string userId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    // ===== Push — skriveoperasjoner (muterer kun change-trackeren, lagrer IKKE) =====

    void AddIngredient(Ingredient ingredient);
    void AddMeal(Meal meal);
    void AddLogEntry(LogEntry logEntry);
    void AddUserGoal(UserGoal goal);

    void RemoveIngredient(Ingredient ingredient);
    void RemoveMeal(Meal meal);
    void RemoveLogEntry(LogEntry logEntry);
    void RemoveUserGoal(UserGoal goal);

    /// <summary>Fjerner komponent-rader ved helerstatning av et måltids innhold.</summary>
    void RemoveMealIngredients(IEnumerable<MealIngredient> components);

    /// <summary>Fjerner budsjett-rader ved helerstatning av et måls budsjetter.</summary>
    void RemoveMealBudgets(IEnumerable<MealBudget> budgets);

    // Barne-rader ved helerstatning på OPPDATERINGS-veien må Add-es eksplisitt
    // via DbSet: Guid-id-ene er satt av klienten, så navigasjons-oppdagelse på en
    // allerede sporet forelder ville merket dem Modified — samme EF-felle som i
    // klientens SaveMealAsync/GoalStore. (Nye foreldre er trygge: Add smitter Added.)

    void AddMealIngredients(IEnumerable<MealIngredient> components);

    void AddMealBudgets(IEnumerable<MealBudget> budgets);

    void AddLogEntryIngredients(IEnumerable<LogEntryIngredient> snapshotRows);

    /// <summary>Fjerner snapshot-rader ved helerstatning av en loggrads innhold.</summary>
    void RemoveLogEntryIngredients(IEnumerable<LogEntryIngredient> snapshotRows);

    // ===== Sletteretten (DELETE /data) og commit =====

    /// <summary>Sletter ALT brukeren har i Calorie-tabellene. Idempotent.</summary>
    Task DeleteAllForUserAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// DET ENE commit-punktet: alle muterte/tillagte/fjernede entiteter
    /// lagres i én implisitt transaksjon. Kalles av servicen, én gang,
    /// til slutt — aldri fra de andre metodene.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
