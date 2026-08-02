using Lyn.Backend.Apps.Calorie.Sync.DTOs.Requests;
using Lyn.Backend.Apps.Calorie.Sync.DTOs.Responses;
using Lyn.Backend.Apps.Calorie.Sync.Mappers;
using Lyn.Backend.Apps.Calorie.Sync.Repositories;
using Lyn.Shared.Enum;
using Lyn.Shared.Result;

namespace Lyn.Backend.Apps.Calorie.Sync.Services;

public class SyncService(ISyncRepository syncRepository, 
    ILogger<SyncService> logger) : ISyncService
{
    public async Task<Result> PushAsync(string userId, SyncPushRequest request, CancellationToken ct = default)
    {
        // Validerer at det er ingen tomme Guid-er eller invalid data
        var validationResult = ValidateRequest(request);
        if (validationResult.IsFailure)
        {
            logger.LogWarning("Sync push from user {UserId} rejected: {Error}", userId, validationResult.Error);
            return validationResult;
        }
        
        // Validerer at ingrediensene brukt til måltider eksisterer i pushen fra klienten eller i databasen
        var validateMealResult = await ValidateMealReferencesAsync(userId, request, ct);
        if (validateMealResult.IsFailure)
        {
            logger.LogWarning("Sync push from user {UserId} rejected: {Error}", userId, validateMealResult.Error);
            return validateMealResult;
        }
        
        // ============ 1. Ingredienser først, da måltider kan være avhengig av den ============
        if (request.Ingredients.Count > 0)
        {
            var existingIngredients =
                await syncRepository.GetIngredientsByIdsAsync(userId,
                    request.Ingredients.Select(x => x.Id).ToList(), ct);

            foreach (var syncIngredient in request.Ingredients)
            {
                // Hvis det er en ny entitet, så legger vi den til eller så oppdaterer vi alle feltene fra requesten
                if (!existingIngredients.TryGetValue(syncIngredient.Id, out var existingIngredient))
                {
                    syncRepository.AddIngredient(syncIngredient.ToEntity(userId));
                }
                else if (!IsIncomingOlder(syncIngredient.UpdatedAtUtc ?? syncIngredient.CreatedAtUtc,
                             existingIngredient.UpdatedAtUtc, existingIngredient.CreatedAtUtc))
                {
                    syncIngredient.ApplyTo(existingIngredient);
                }
            }
        }

        // ============ 2. Måltider ============
        if (request.Meals.Count > 0)
        {
            var existingMeals =
                await syncRepository.GetMealsByIdsAsync(userId,
                    request.Meals.Select(x => x.Id).ToList(), ct);

            foreach (var syncMeal in request.Meals)
            {
                // Hvis det er en ny entitet, så legger vi den til eller så oppdaterer vi alle feltene fra requesten
                if (!existingMeals.TryGetValue(syncMeal.Id, out var existingMeal))
                {
                    syncRepository.AddMeal(syncMeal.ToEntity(userId));
                }
                else if (!IsIncomingOlder(syncMeal.UpdatedAtUtc ?? syncMeal.CreatedAtUtc,
                             existingMeal.UpdatedAtUtc, existingMeal.CreatedAtUtc))
                {
                    syncMeal.ApplyTo(existingMeal);

                    // Vi endrer den oppdaterte entiteten til å matche klienten, deretter fjerner vi og legger til
                    // komponent ingrediensene for å være sikker på at de matcher med det fra klienten
                    syncRepository.RemoveMealIngredients(existingMeal.Ingredients.ToList());
                    existingMeal.Ingredients.Clear();
                    syncRepository.AddMealIngredients(
                        syncMeal.Components.Select(c => c.ToEntity(syncMeal.Id)).ToList());
                }
            }
        }

        // ============ 3. LogEntries ============
        if (request.LogEntries.Count > 0)
        {
            var existingLogEntries =
                await syncRepository.GetLogEntriesByIdsAsync(userId,
                    request.LogEntries.Select(x => x.Id).ToList(), ct);
        
            foreach (var syncLogEntry in request.LogEntries)
            {
                // Hvis det er en ny entitet, så legger vi den til eller så oppdaterer vi alle feltene fra requesten
                if (!existingLogEntries.TryGetValue(syncLogEntry.Id, out var existingLogEntry))
                {
                    syncRepository.AddLogEntry(syncLogEntry.ToEntity(userId));
                }
                else if (!IsIncomingOlder(syncLogEntry.UpdatedAtUtc ?? syncLogEntry.LoggedAtUtc,
                             existingLogEntry.UpdatedAtUtc, existingLogEntry.LoggedAtUtc))
                {
                    // Vi endrer den oppdaterte entiteten til å matche klienten, deretter fjerner vi og legger til
                    // logentry ingrediensene for å være sikker på at de matcher med det fra klienten
                    syncLogEntry.ApplyTo(existingLogEntry); 
                    syncRepository.RemoveLogEntryIngredients(existingLogEntry.Ingredients.ToList()); 
                    existingLogEntry.Ingredients.Clear();
                    syncRepository.AddLogEntryIngredients(
                        [syncLogEntry.ToSnapshotRow(existingLogEntry.Id)]);
                }
            }
        }
        
        // ============ 4. Goals ============
        if (request.Goals.Count > 0)
        {
            var existingGoals =
                await syncRepository.GetGoalsByIdsAsync(userId,
                    request.Goals.Select(x => x.Id).ToList(), ct);
        
            foreach (var syncGoal in request.Goals)
            {
                // Hvis det er en ny entitet, så legger vi den til eller så oppdaterer vi alle feltene fra requesten
                if (!existingGoals.TryGetValue(syncGoal.Id, out var existingGoal))
                {
                    syncRepository.AddUserGoal(syncGoal.ToEntity(userId));
                }
                else if (!IsIncomingOlder(syncGoal.UpdatedAtUtc ?? syncGoal.CreatedAtUtc,
                             existingGoal.UpdatedAtUtc, existingGoal.CreatedAtUtc))
                {
                    // Serverens gamle ut, klientens nye inn
                    syncGoal.ApplyTo(existingGoal); 
                    syncRepository.RemoveMealBudgets(existingGoal.MealBudgets.ToList());
                    existingGoal.MealBudgets.Clear();
                    syncRepository.AddMealBudgets(
                        syncGoal.MealBudgets.Select(b => b.ToEntity(syncGoal.Id)).ToList());
                }
            }
        }
        
        // ============ 5. Sletting ============

        if (request.Deleted.MealIds.Count > 0)
        {
            var mealsToDelete = await syncRepository.GetMealsByIdsAsync(userId, 
                request.Deleted.MealIds, ct);
            foreach (var meal in mealsToDelete.Values)
            {
                syncRepository.RemoveMeal(meal);
            }
        }
        
        if (request.Deleted.IngredientIds.Count > 0)
        {
            var ingredientsToDelete = await syncRepository.GetIngredientsByIdsAsync(userId, 
                request.Deleted.IngredientIds, ct);
            foreach (var ingredient in ingredientsToDelete.Values)
            {
                syncRepository.RemoveIngredient(ingredient);
            }
        }
        
        if (request.Deleted.LogEntryIds.Count > 0)
        {
            var logEntriesToDelete = await syncRepository.GetLogEntriesByIdsAsync(userId, 
                request.Deleted.LogEntryIds, ct);
            foreach (var logEntry in logEntriesToDelete.Values)
            {
                syncRepository.RemoveLogEntry(logEntry);
            }
        }
        
        if (request.Deleted.GoalIds.Count > 0)
        {
            var goalsToDelete = await syncRepository.GetGoalsByIdsAsync(userId, 
                request.Deleted.GoalIds, ct);
            foreach (var userGoal in goalsToDelete.Values)
            {
                syncRepository.RemoveUserGoal(userGoal);
            }
        }
        

        // ============ 6. Commit — hele batchen som ÉN transaksjon ============
        await syncRepository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<SyncStateResponse>> GetStateAsync(string userId, CancellationToken ct = default)
    {
        var ingredients = await syncRepository.GetAllIngredientsAsync(userId, ct);
        var meals = await syncRepository.GetAllMealsAsync(userId, ct);
        var logEntries = await syncRepository.GetAllLogEntriesAsync(userId, ct);
        var goals = await syncRepository.GetAllGoalsAsync(userId, ct);

        return Result<SyncStateResponse>.Success(new SyncStateResponse
        {
            Ingredients = ingredients.Select(ingredient => ingredient.ToDto()).ToList(),
            Meals = meals.Select(meal => meal.ToDto()).ToList(),
            LogEntries = logEntries.Select(logEntry => logEntry.ToDto()).ToList(),
            Goals = goals.Select(goal => goal.ToDto()).ToList(),
        });
    }

    public async Task<Result> DeleteAllDataAsync(string userId, CancellationToken ct = default)
    {
        await syncRepository.DeleteAllForUserAsync(userId, ct);
        logger.LogInformation("All Calorie sync data deleted for user {UserId}", userId);
        return Result.Success();
    }
    
    
    // =============================== Hjelpemetoder ===============================
    
    /// <summary>
    /// Kontraktsvalidering: DTO-ene har ingen genererende defaults, så manglende
    /// felt fra en buggy klient blir Guid.Empty / år 0001 — synlig søppel som
    /// avvises her med presis forklaring, i stedet for å bli plausible rader.
    /// </summary>
    private static Result ValidateRequest(SyncPushRequest request)
    {
        if (request.Ingredients.Any(i => i.Id == Guid.Empty || i.CreatedAtUtc == default))
            return Result.Failure("Push contains an ingredient with missing Id or CreatedAtUtc.", 
                AppErrorCode.Validation);
        
        if (request.Meals.Any(m => m.Id == Guid.Empty || m.CreatedAtUtc == default))
            return Result.Failure("Push contains a meal with missing Id or CreatedAtUtc.", 
                AppErrorCode.Validation);

        if (request.Meals.SelectMany(m => m.Components).Any(c => c.Id == Guid.Empty 
                                                                 || c.IngredientId == Guid.Empty))
            return Result.Failure("Push contains a meal component with missing Id or IngredientId.", 
                AppErrorCode.Validation);

        if (request.LogEntries.Any(e => e.Id == Guid.Empty 
                                        || e.LoggedAtUtc == default 
                                        || e.LoggedDate == default
                                        || e.AmountGrams <= 0)) // Sikrer oss at antall gram er høyere enn 0
            return Result.Failure("Push contains a log entry with missing Id, LoggedAtUtc, LoggedDate " +
                                  "or non-positive AmountGrams.",
                AppErrorCode.Validation);

        if (request.Goals.Any(g => g.Id == Guid.Empty 
                                   || g.CreatedAtUtc == default 
                                   || g.EffectiveFromDate == default))
            return Result.Failure("Push contains a goal with missing Id, CreatedAtUtc or EffectiveFromDate.",
                AppErrorCode.Validation);

        if (request.Goals.SelectMany(g => g.MealBudgets).Any(b => b.Id == Guid.Empty))
            return Result.Failure("Push contains a meal budget with missing Id.", AppErrorCode.Validation);

        var deleted = request.Deleted;
        if (deleted.IngredientIds.Concat(deleted.MealIds).Concat(deleted.LogEntryIds).Concat(deleted.GoalIds)
            .Any(id => id == Guid.Empty))
            return Result.Failure("Push contains a tombstone with an empty id.", AppErrorCode.Validation);

        return Result.Success();
    }
    
    /// <summary>
    /// Data-avhengig validering: Har alle måltidenes ingredienser en entitet? Hver komponent
    /// må referere en ingrediens som enten er med i pushen eller allerede
    /// finnes hos brukeren — ellers ville SaveChanges smelt i FK-en.
    /// </summary>
    private async Task<Result> ValidateMealReferencesAsync(string userId, SyncPushRequest request,
        CancellationToken ct)
    {
        // Ingredienser med i pushen fra klienten - HashSet for rask gjennomgang
        var syncedIngredientIds = request.Ingredients.Select(i => i.Id).ToHashSet();
        
        // Ingredisenser fra klineten brukt i Meals
        var referencedIngredientsInMealsNotInPush = request.Meals.SelectMany(m => m.Components)
            .Select(c => c.IngredientId)
            .Where(id => !syncedIngredientIds.Contains(id))
            .Distinct()
            .ToList();
        
        // Hvis klienten ikke har sendt med ingredienser som SyncIngredientDto, men er med i SyncMealDto-ene, 
        // må vi da sjekke at ingrediensene er synket inn i databasen tidligere, eller så har det oppstått en feil
        if (referencedIngredientsInMealsNotInPush.Count > 0)
        {
            var existingIngredients = await syncRepository.GetIngredientsByIdsAsync(userId, 
                referencedIngredientsInMealsNotInPush, ct);
            syncedIngredientIds.UnionWith(existingIngredients.Keys);
        }

        var unknownIngredient = request.Meals
            .SelectMany(m => m.Components)
            .Select(c => c.IngredientId)
            .FirstOrDefault(id => !syncedIngredientIds.Contains(id));
        
        if (unknownIngredient != Guid.Empty)
            return Result.Failure($"Meal component references unknown ingredient '{unknownIngredient}'.",
                AppErrorCode.Validation);

        return Result.Success();
    }
    
    /// <summary>
    /// Sjekker om innkommende entitet fra klienten er nyere eller eldre enn entiteten i database.
    /// Er entiteten nyere, så oppdaterer vi entiteten i databasen, eller så hopper vi over
    /// </summary>
    /// <param name="incoming">UpdatedAt/CreatedAt fra klienten</param>
    /// <param name="existingUpdated">Nullable UpdatedAt for entiteter som har blitt endret etter opprettelse</param>
    /// <param name="existingCreated">Opprettesetiden på entiteten fra databasen</param>
    /// <returns>True hvis databasen sin er nyere og vi hopper over,
    ///   false hvis vi må oppatere med entiteten fra klienten</returns>
    private static bool IsIncomingOlder(DateTime incoming, DateTime? existingUpdated, DateTime existingCreated) =>
        incoming < (existingUpdated ?? existingCreated);
}
