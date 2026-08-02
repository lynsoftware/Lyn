using Lyn.Backend.Apps.Calorie.Models;
using Lyn.Backend.Apps.Calorie.Sync.DTOs;

namespace Lyn.Backend.Apps.Calorie.Sync.Mappers;

/// <summary>
/// Mapping begge veier mellom entiteter og sync-kontrakten. ToDto (state):
/// owned type flates ut, målets feltnavn oversettes, loggens per-100g-barn
/// summeres til totaler. ToEntity/ApplyTo (push): ToEntity føder raden
/// (Id/UserId/CreatedAtUtc settes ÉN gang og røres aldri igjen), ApplyTo
/// overskriver resten — feltlisten finnes dermed ett sted. UpdatedAtUtc
/// kopieres alltid fra klienten (aldri DateTime.UtcNow): replikaen skal
/// huske KLIENTENS tidsstempel, ellers bryter LWW på tvers av enheter.
/// </summary>
public static class SyncMappingExtensions
{
    /// <summary>
    /// Mapper en Ingredient-entitet til en SyncIngredientDto
    /// </summary>
    public static SyncIngredientDto ToDto(this Ingredient ingredient) => new()
    {
        Id = ingredient.Id,
        Name = ingredient.Name,
        Brand = ingredient.Brand,
        CaloriesPer100g = ingredient.Nutrition.Calories,
        ProteinPer100g = ingredient.Nutrition.Protein,
        CarbsPer100g = ingredient.Nutrition.Carbs,
        FatPer100g = ingredient.Nutrition.Fat,
        UnitWeightGrams = ingredient.UnitWeightGrams,
        UnitName = ingredient.UnitName,
        CreatedAtUtc = ingredient.CreatedAtUtc,
        UpdatedAtUtc = ingredient.UpdatedAtUtc
    };
    
    /// <summary>
    /// Mapper en Meal-entitet til en SyncMealDto
    /// </summary>
    public static SyncMealDto ToDto(this Meal meal) => new()
    {
        Id = meal.Id,
        Name = meal.Name,
        Brand  = meal.Brand,
        Components = meal.Ingredients.Select(c => c.ToDto()).ToList(),
        CreatedAtUtc = meal.CreatedAtUtc,
        UpdatedAtUtc = meal.UpdatedAtUtc
    };
    
    /// <summary>
    /// Mapper en MealIngredient-entitet til en SyncMealComponentDto
    /// </summary>
    public static SyncMealComponentDto ToDto(this MealIngredient component) => new()
    {
        Id = component.Id,
        IngredientId = component.IngredientId,
        AmountGrams = component.AmountGrams
    };
    
    /// <summary>
    /// Mapper en LogEntry-entitet til en SyncLogEntryDto
    /// </summary>
    public static SyncLogEntryDto ToDto(this LogEntry entry) => new()
    {
        Id = entry.Id,
        MealType = entry.MealType,
        LoggedDate = entry.LoggedDate,
        LoggedAtUtc = entry.LoggedAtUtc,
        UpdatedAtUtc = entry.UpdatedAtUtc,
        Name = entry.Name,
        // Serveren lagrer per 100 g + mengde; klientens flate rad vil ha
        // totaler — summert over snapshot-barna (total = per100 x gram / 100)
        AmountGrams = entry.Ingredients.Sum(i => i.AmountGrams),
        Calories = (int)Math.Round(entry.Ingredients.Sum(i => i.AmountGrams * i.Nutrition.Calories / 100m)),
        ProteinGrams = entry.Ingredients.Sum(i => i.AmountGrams * i.Nutrition.Protein / 100m),
        CarbsGrams = entry.Ingredients.Sum(i => i.AmountGrams * i.Nutrition.Carbs / 100m),
        FatGrams = entry.Ingredients.Sum(i => i.AmountGrams * i.Nutrition.Fat / 100m)
    };
    
    /// <summary>
    /// Mapper en UserGoal-entitet til en SyncUserGoalDto
    /// </summary>
    public static SyncUserGoalDto ToDto(this UserGoal goal) => new()
    {
        Id = goal.Id,
        DailyCalories = goal.DailyCalories,
        Mode = goal.Mode,
        // Navnespriket entitet ↔ kontrakt absorberes her — det er mapperens jobb
        ProteinGoalGrams = goal.ProteinGrams,
        CarbsGoalGrams = goal.CarbsGrams,
        FatGoalGrams = goal.FatGrams,
        TreatPercent = goal.TreatPercent,
        EffectiveFromDate = goal.EffectiveFromDate,
        CreatedAtUtc = goal.CreatedAtUtc,
        UpdatedAtUtc = goal.UpdatedAtUtc,
        MealBudgets = goal.MealBudgets.Select(b => b.ToDto()).ToList()
    };
    
    /// <summary>
    /// Mapper en MealBudget-entitet til en SyncMealBudgetDto
    /// </summary>
    public static SyncMealBudgetDto ToDto(this MealBudget budget) => new()
    {
        Id = budget.Id,
        MealType = budget.MealType,
        MaxCalories = budget.MaxCalories
    };

    // ===== Push: DTO → entitet =====
    
    /// <summary>
    /// Mapper da en SyncIngredientDto-dto til en Ingredient-entitet
    /// </summary>
    public static Ingredient ToEntity(this SyncIngredientDto dto, string userId)
    {
        var entity = new Ingredient { Id = dto.Id, UserId = userId, CreatedAtUtc = dto.CreatedAtUtc };
        dto.ApplyTo(entity);
        return entity;
    }
    
    /// <summary>
    /// Setter egenskapene til en Ingredient-entity fra en SyncIngredientDto. Egen metode som brukes til både oppretting
    /// og endring av en entitet
    /// </summary>
    /// <param name="dto">SyncIngredientDto vi tar egenskapene fra</param>
    /// <param name="entity">Entiteten vi setter egenskapene til</param>
    public static void ApplyTo(this SyncIngredientDto dto, Ingredient entity)
    {
        entity.Name = dto.Name;
        entity.Brand = dto.Brand;
        // Owned type: mutér feltene, aldri bytt hele objektet — tryggest for change-trackeren
        entity.Nutrition.Calories = dto.CaloriesPer100g;
        entity.Nutrition.Protein = dto.ProteinPer100g;
        entity.Nutrition.Carbs = dto.CarbsPer100g;
        entity.Nutrition.Fat = dto.FatPer100g;
        entity.UnitWeightGrams = dto.UnitWeightGrams;
        entity.UnitName = dto.UnitName;
        entity.UpdatedAtUtc = dto.UpdatedAtUtc;
    }

    /// <summary>
    /// Nytt måltid med komponentene — Add på forelderen smitter Added til
    /// hele grafen, så barna er trygge her (i motsetning til erstatnings-
    /// veien i servicen, som må Add-e barna eksplisitt via DbSet).
    /// </summary>
    public static Meal ToEntity(this SyncMealDto dto, string userId)
    {
        var entity = new Meal { Id = dto.Id, UserId = userId, CreatedAtUtc = dto.CreatedAtUtc };
        dto.ApplyTo(entity);

        foreach (var component in dto.Components)
            entity.Ingredients.Add(component.ToEntity(dto.Id));

        return entity;
    }

    /// <summary>Kun måltidets egne felt — komponenterstattningen eies av servicen.</summary>
    public static void ApplyTo(this SyncMealDto dto, Meal entity)
    {
        entity.Name = dto.Name;
        entity.Brand = dto.Brand;
        entity.UpdatedAtUtc = dto.UpdatedAtUtc;
    }

    public static MealIngredient ToEntity(this SyncMealComponentDto dto, Guid mealId) => new()
    {
        Id = dto.Id,
        MealId = mealId,
        IngredientId = dto.IngredientId,
        AmountGrams = dto.AmountGrams
    };

    public static LogEntry ToEntity(this SyncLogEntryDto dto, string userId)
    {
        var entity = new LogEntry
        {
            Id = dto.Id,
            UserId = userId,
            LoggedAtUtc = dto.LoggedAtUtc,
            LoggedDate = dto.LoggedDate
        };
        dto.ApplyTo(entity);
        entity.Ingredients.Add(dto.ToSnapshotRow(dto.Id));
        return entity;
    }

    /// <summary>Kun loggradens egne felt — snapshot-barnet erstattes av servicen.</summary>
    public static void ApplyTo(this SyncLogEntryDto dto, LogEntry entity)
    {
        entity.Name = dto.Name;
        entity.MealType = dto.MealType;
        entity.LoggedDate = dto.LoggedDate;
        entity.UpdatedAtUtc = dto.UpdatedAtUtc;
    }

    /// <summary>
    /// Klientens flate totaler → serverens per-100g-snapshot (motsatt av ToDto:
    /// per100 = total x 100 / gram). Barnet er server-eid — klienten kjenner
    /// ingen barne-id, så den genereres her; ved erstatning får raden ny id,
    /// som er harmløst siden ingenting refererer den.
    /// </summary>
    public static LogEntryIngredient ToSnapshotRow(this SyncLogEntryDto dto, Guid logEntryId)
    {
        // Valideringen garanterer AmountGrams > 0 — guarden er belte og bukse
        var grams = dto.AmountGrams > 0 ? dto.AmountGrams : 1;

        return new LogEntryIngredient
        {
            Id = Guid.NewGuid(),
            LogEntryId = logEntryId,
            IngredientName = dto.Name,
            AmountGrams = dto.AmountGrams,
            Nutrition = new NutritionPer100G
            {
                Calories = dto.Calories * 100m / grams,
                Protein = dto.ProteinGrams * 100m / grams,
                Carbs = dto.CarbsGrams * 100m / grams,
                Fat = dto.FatGrams * 100m / grams
            }
        };
    }

    public static UserGoal ToEntity(this SyncUserGoalDto dto, string userId)
    {
        var entity = new UserGoal { Id = dto.Id, UserId = userId, CreatedAtUtc = dto.CreatedAtUtc };
        dto.ApplyTo(entity);

        foreach (var budget in dto.MealBudgets)
            entity.MealBudgets.Add(budget.ToEntity(dto.Id));

        return entity;
    }

    /// <summary>Kun målets egne felt — budsjett-erstattningen eies av servicen.</summary>
    public static void ApplyTo(this SyncUserGoalDto dto, UserGoal entity)
    {
        entity.DailyCalories = dto.DailyCalories;
        entity.Mode = dto.Mode;
        // Navnespriket motsatt vei: kontraktens GoalGrams → entitetens Grams
        entity.ProteinGrams = dto.ProteinGoalGrams;
        entity.CarbsGrams = dto.CarbsGoalGrams;
        entity.FatGrams = dto.FatGoalGrams;
        entity.TreatPercent = dto.TreatPercent;
        entity.EffectiveFromDate = dto.EffectiveFromDate;
        entity.UpdatedAtUtc = dto.UpdatedAtUtc;
    }

    public static MealBudget ToEntity(this SyncMealBudgetDto dto, Guid userGoalId) => new()
    {
        Id = dto.Id,
        UserGoalId = userGoalId,
        MealType = dto.MealType,
        MaxCalories = dto.MaxCalories
    };
}