using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lyn.Backend.Apps.Calorie.Enums;
using Lyn.Backend.Apps.Calorie.Models;
using Lyn.Backend.Apps.Calorie.Sync.DTOs;
using Lyn.Backend.Apps.Calorie.Sync.DTOs.Requests;
using Lyn.Backend.Apps.Calorie.Sync.DTOs.Responses;

namespace Lyn.Tests.Backend.Integrations.IntegrationsTests;

/// <summary>
/// Integrasjonstester for sync-endepunktene mot ekte PostgreSQL.
/// Viktigst av alt: eierskapsfilteret — state skal ALDRI lekke en annen
/// brukers rader.
/// </summary>
[Collection(nameof(IntegrationTestsCollection))]
public class SyncControllerIntegrationTests : IAsyncLifetime
{
    private const string StateUrl = "/api/calorie/sync/state";
    private const string PushUrl = "/api/calorie/sync/push";
    private const string DataUrl = "/api/calorie/sync/data";

    // Fast klokke i testene — Npgsql krever DateTimeKind.Utc
    private static readonly DateTime T0 = new(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc);

    private readonly LynBackendApplicationFactory _factory;
    private readonly HttpClient _client;

    public SyncControllerIntegrationTests(LynBackendApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

    private void AuthenticateAs(string userId) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _factory.CreateJwtFor(userId));

    [Fact]
    public async Task GetState_WithoutToken_Returns401()
    {
        // Act — ingen Authorization-header
        var response = await _client.GetAsync(StateUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetState_WhenNewUser_ReturnsEmptyState()
    {
        // Arrange — bruker uten data: tom state er et GYLDIG svar, aldri 404
        AuthenticateAs("user-with-no-data");

        // Act
        var response = await _client.GetAsync(StateUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var state = await response.Content.ReadFromJsonAsync<SyncStateResponse>();
        state!.Ingredients.Should().BeEmpty();
        state.Meals.Should().BeEmpty();
        state.LogEntries.Should().BeEmpty();
        state.Goals.Should().BeEmpty();
    }

    [Fact]
    public async Task GetState_WhenSeededData_ReturnsOwnRowsWithNestedChildren()
    {
        // Arrange — bruker A har alt av datatyper, bruker B har én ingrediens
        // som ALDRI skal dukke opp i A sin state
        const string userA = "user-a";
        const string userB = "user-b";

        var ingredientId = Guid.NewGuid();
        var mealId = Guid.NewGuid();

        await _factory.SeedCalorieAsync(async db =>
        {
            db.Ingredients.Add(new Ingredient
            {
                Id = ingredientId,
                UserId = userA,
                Name = "Havregryn",
                Brand = "Axa",
                Nutrition = new NutritionPer100G { Calories = 370, Protein = 13, Carbs = 60, Fat = 7 },
                UnitWeightGrams = 40,
                UnitName = "dl"
            });

            db.Meals.Add(new Meal
            {
                Id = mealId,
                UserId = userA,
                Name = "Havregrøt",
                Brand = "Hjemmelaget",
                Ingredients = { new MealIngredient { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 120 } }
            });

            db.LogEntries.Add(new LogEntry
            {
                Id = Guid.NewGuid(),
                UserId = userA,
                Name = "Havregrøt",
                MealType = MealType.Breakfast,
                LoggedDate = new DateOnly(2026, 7, 27),
                // Snapshot lagres per 100 g — endepunktet skal returnere TOTALER
                Ingredients =
                {
                    new LogEntryIngredient
                    {
                        Id = Guid.NewGuid(),
                        IngredientName = "Havregryn",
                        AmountGrams = 50,
                        Nutrition = new NutritionPer100G { Calories = 370, Protein = 13, Carbs = 60, Fat = 7 }
                    }
                }
            });

            db.UserGoals.Add(new UserGoal
            {
                Id = Guid.NewGuid(),
                UserId = userA,
                DailyCalories = 2000,
                Mode = GoalMode.Limit,
                TreatPercent = 20,
                EffectiveFromDate = new DateOnly(2026, 7, 1),
                MealBudgets = { new MealBudget { Id = Guid.NewGuid(), MealType = MealType.Dinner, MaxCalories = 600 } }
            });

            // Bruker B — lekkasjetesten
            db.Ingredients.Add(new Ingredient
            {
                Id = Guid.NewGuid(),
                UserId = userB,
                Name = "Fremmed ingrediens",
                Nutrition = new NutritionPer100G { Calories = 100 }
            });

            await db.SaveChangesAsync();
        });

        AuthenticateAs(userA);

        // Act
        var response = await _client.GetAsync(StateUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<SyncStateResponse>();

        // Eierskap: kun A sine rader — B sin ingrediens finnes ikke i svaret
        var ingredient = state!.Ingredients.Should().ContainSingle().Subject;
        ingredient.Id.Should().Be(ingredientId);
        ingredient.Name.Should().Be("Havregryn");
        ingredient.Brand.Should().Be("Axa");
        ingredient.CaloriesPer100g.Should().Be(370);
        ingredient.UnitWeightGrams.Should().Be(40);

        // Måltid med nøstet komponent — og Brand overlever rundturen
        var meal = state.Meals.Should().ContainSingle().Subject;
        meal.Id.Should().Be(mealId);
        meal.Brand.Should().Be("Hjemmelaget");
        var component = meal.Components.Should().ContainSingle().Subject;
        component.IngredientId.Should().Be(ingredientId);
        component.AmountGrams.Should().Be(120);

        // Logg: per-100g-snapshotet er regnet om til totaler (50 g x 370/100 = 185)
        var logEntry = state.LogEntries.Should().ContainSingle().Subject;
        logEntry.LoggedDate.Should().Be(new DateOnly(2026, 7, 27));
        logEntry.AmountGrams.Should().Be(50);
        logEntry.Calories.Should().Be(185);
        logEntry.ProteinGrams.Should().Be(6.5m);
        logEntry.CarbsGrams.Should().Be(30m);
        logEntry.FatGrams.Should().Be(3.5m);

        // Mål med nøstet budsjett og navnesprik-mappingen (ProteinGrams → ProteinGoalGrams)
        var goal = state.Goals.Should().ContainSingle().Subject;
        goal.DailyCalories.Should().Be(2000);
        goal.TreatPercent.Should().Be(20);
        goal.MealBudgets.Should().ContainSingle()
            .Which.MaxCalories.Should().Be(600);
    }

    // ===== Push =====

    [Fact]
    public async Task Push_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync(PushUrl, new SyncPushRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Push_RoundTrip_StateReturnsExactlyWhatWasPushed()
    {
        // Arrange — en push med alle fire datatyper, nøstet og koblet
        AuthenticateAs("roundtrip-user");

        var ingredientId = Guid.NewGuid();
        var mealId = Guid.NewGuid();
        var logEntryId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        var push = new SyncPushRequest
        {
            Ingredients =
            [
                new SyncIngredientDto
                {
                    Id = ingredientId, Name = "Havregryn", Brand = "Axa",
                    CaloriesPer100g = 370, ProteinPer100g = 13, CarbsPer100g = 60, FatPer100g = 7,
                    UnitWeightGrams = 40, UnitName = "dl", CreatedAtUtc = T0
                }
            ],
            Meals =
            [
                new SyncMealDto
                {
                    Id = mealId, Name = "Havregrøt", Brand = "Hjemmelaget", CreatedAtUtc = T0,
                    Components = [new SyncMealComponentDto
                        { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 120 }]
                }
            ],
            LogEntries =
            [
                new SyncLogEntryDto
                {
                    Id = logEntryId, Name = "Havregrøt", MealType = MealType.Breakfast,
                    LoggedDate = new DateOnly(2026, 7, 27), LoggedAtUtc = T0,
                    AmountGrams = 50, Calories = 185, ProteinGrams = 6.5m, CarbsGrams = 30m, FatGrams = 3.5m
                }
            ],
            Goals =
            [
                new SyncUserGoalDto
                {
                    Id = goalId, DailyCalories = 2000, Mode = GoalMode.Limit, TreatPercent = 20,
                    EffectiveFromDate = new DateOnly(2026, 7, 1), CreatedAtUtc = T0,
                    MealBudgets = [new SyncMealBudgetDto
                        { Id = Guid.NewGuid(), MealType = MealType.Dinner, MaxCalories = 600 }]
                }
            ]
        };

        // Act — push inn, state ut
        var pushResponse = await _client.PostAsJsonAsync(PushUrl, push);
        pushResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var state = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();

        // Assert — rundturen skal være tapsfri
        var ingredient = state!.Ingredients.Should().ContainSingle().Subject;
        ingredient.Id.Should().Be(ingredientId);
        ingredient.Brand.Should().Be("Axa");
        ingredient.CaloriesPer100g.Should().Be(370);

        var meal = state.Meals.Should().ContainSingle().Subject;
        meal.Brand.Should().Be("Hjemmelaget");
        meal.Components.Should().ContainSingle().Which.IngredientId.Should().Be(ingredientId);

        // Loggen tåler totaler → per-100g → totaler uten avvik
        var logEntry = state.LogEntries.Should().ContainSingle().Subject;
        logEntry.AmountGrams.Should().Be(50);
        logEntry.Calories.Should().Be(185);
        logEntry.ProteinGrams.Should().Be(6.5m);

        state.Goals.Should().ContainSingle()
            .Which.MealBudgets.Should().ContainSingle()
            .Which.MaxCalories.Should().Be(600);
    }

    [Fact]
    public async Task Push_WhenIncomingIsOlder_LwwKeepsNewerRow()
    {
        // Arrange — samme rad fra to "enheter": først den nye, så en eldre etternøler
        AuthenticateAs("lww-user");
        var id = Guid.NewGuid();

        SyncIngredientDto Row(string name, DateTime? updated) => new()
        {
            Id = id, Name = name, CaloriesPer100g = 100, CreatedAtUtc = T0, UpdatedAtUtc = updated
        };

        await _client.PostAsJsonAsync(PushUrl,
            new SyncPushRequest { Ingredients = [Row("Nyeste", T0.AddMinutes(30))] });

        // Act — enheten med den GAMLE endringen pusher etterpå
        var response = await _client.PostAsJsonAsync(PushUrl,
            new SyncPushRequest { Ingredients = [Row("Gammel etternøler", T0.AddMinutes(10))] });

        // Assert — pushen er OK (skip er protokoll, ikke feil), men raden står urørt
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var state = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();
        state!.Ingredients.Should().ContainSingle().Which.Name.Should().Be("Nyeste");
    }

    [Fact]
    public async Task Push_WhenIncomingIsNewer_LwwOverwritesRowAndChildren()
    {
        // Arrange — måltid pushes, deretter en nyere versjon med endret komponent
        AuthenticateAs("lww-newer-user");
        var ingredientId = Guid.NewGuid();
        var mealId = Guid.NewGuid();

        var first = new SyncPushRequest
        {
            Ingredients = [new SyncIngredientDto
                { Id = ingredientId, Name = "Egg", CaloriesPer100g = 150, CreatedAtUtc = T0 }],
            Meals = [new SyncMealDto
            {
                Id = mealId, Name = "Omelett", CreatedAtUtc = T0,
                Components = [new SyncMealComponentDto
                    { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 120 }]
            }]
        };
        await _client.PostAsJsonAsync(PushUrl, first);

        var second = new SyncPushRequest
        {
            Meals = [new SyncMealDto
            {
                Id = mealId, Name = "Stor omelett", CreatedAtUtc = T0, UpdatedAtUtc = T0.AddHours(1),
                Components = [new SyncMealComponentDto
                    { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 180 }]
            }]
        };

        // Act
        var response = await _client.PostAsJsonAsync(PushUrl, second);

        // Assert — både forelder og helerstattede barn er oppdatert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var state = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();
        var meal = state!.Meals.Should().ContainSingle().Subject;
        meal.Name.Should().Be("Stor omelett");
        meal.Components.Should().ContainSingle().Which.AmountGrams.Should().Be(180);
    }

    [Fact]
    public async Task Push_WithTombstones_DeletesRowsAndIgnoresUnknownIds()
    {
        // Arrange — måltid + ingrediens inn, deretter tombstones for begge
        // pluss en id som aldri har eksistert (retry-idempotens)
        AuthenticateAs("tombstone-user");
        var ingredientId = Guid.NewGuid();
        var mealId = Guid.NewGuid();

        await _client.PostAsJsonAsync(PushUrl, new SyncPushRequest
        {
            Ingredients = [new SyncIngredientDto
                { Id = ingredientId, Name = "Egg", CaloriesPer100g = 150, CreatedAtUtc = T0 }],
            Meals = [new SyncMealDto
            {
                Id = mealId, Name = "Omelett", CreatedAtUtc = T0,
                Components = [new SyncMealComponentDto
                    { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 120 }]
            }]
        });

        // Act — sletting i samme rekkefølge klienten ville sendt den
        var response = await _client.PostAsJsonAsync(PushUrl, new SyncPushRequest
        {
            Deleted = new SyncDeletedDto
            {
                MealIds = [mealId],
                IngredientIds = [ingredientId, Guid.NewGuid()]   // ukjent id skal ignoreres stille
            }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var state = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();
        state!.Meals.Should().BeEmpty();
        state.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public async Task Push_WithEmptyGuid_Returns422()
    {
        // Arrange — kontraktssøppel: manglende Id blir Guid.Empty
        AuthenticateAs("garbage-user");

        var push = new SyncPushRequest
        {
            Ingredients = [new SyncIngredientDto
                { Id = Guid.Empty, Name = "Ødelagt", CreatedAtUtc = T0 }]
        };

        // Act
        var response = await _client.PostAsJsonAsync(PushUrl, push);

        // Assert — AppErrorCode.Validation → 422 i kontrakten (BuildProblemResult)
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Push_WhenMealReferencesUnknownIngredient_Returns422()
    {
        // Arrange — komponenten peker på en ingrediens som verken er i pushen
        // eller finnes hos brukeren (sletterett/kappløp-scenarioene)
        AuthenticateAs("unknown-ref-user");

        var push = new SyncPushRequest
        {
            Meals = [new SyncMealDto
            {
                Id = Guid.NewGuid(), Name = "Foreldreløst måltid", CreatedAtUtc = T0,
                Components = [new SyncMealComponentDto
                    { Id = Guid.NewGuid(), IngredientId = Guid.NewGuid(), AmountGrams = 100 }]
            }]
        };

        // Act
        var response = await _client.PostAsJsonAsync(PushUrl, push);

        // Assert — presis 422, ikke FK-krasj
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ===== Sletteretten =====

    [Fact]
    public async Task DeleteData_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.DeleteAsync(DataUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteData_RemovesAllOwnDataButNeverOthers()
    {
        // Arrange — bruker A har alle fire datatyper, bruker B har én ingrediens
        const string userA = "delete-user-a";
        const string userB = "delete-user-b";

        var ingredientId = Guid.NewGuid();

        AuthenticateAs(userA);
        await _client.PostAsJsonAsync(PushUrl, new SyncPushRequest
        {
            Ingredients = [new SyncIngredientDto
                { Id = ingredientId, Name = "Egg", CaloriesPer100g = 150, CreatedAtUtc = T0 }],
            Meals = [new SyncMealDto
            {
                Id = Guid.NewGuid(), Name = "Omelett", CreatedAtUtc = T0,
                Components = [new SyncMealComponentDto
                    { Id = Guid.NewGuid(), IngredientId = ingredientId, AmountGrams = 120 }]
            }],
            LogEntries = [new SyncLogEntryDto
            {
                Id = Guid.NewGuid(), Name = "Omelett", MealType = MealType.Breakfast,
                LoggedDate = new DateOnly(2026, 7, 27), LoggedAtUtc = T0,
                AmountGrams = 120, Calories = 180, ProteinGrams = 15m, CarbsGrams = 1m, FatGrams = 12m
            }],
            Goals = [new SyncUserGoalDto
            {
                Id = Guid.NewGuid(), DailyCalories = 2000, Mode = GoalMode.Limit,
                EffectiveFromDate = new DateOnly(2026, 7, 1), CreatedAtUtc = T0,
                MealBudgets = [new SyncMealBudgetDto
                    { Id = Guid.NewGuid(), MealType = MealType.Dinner, MaxCalories = 600 }]
            }]
        });

        AuthenticateAs(userB);
        await _client.PostAsJsonAsync(PushUrl, new SyncPushRequest
        {
            Ingredients = [new SyncIngredientDto
                { Id = Guid.NewGuid(), Name = "Overlever", CaloriesPer100g = 100, CreatedAtUtc = T0 }]
        });

        // Act — A bruker sletteretten
        AuthenticateAs(userA);
        var deleteResponse = await _client.DeleteAsync(DataUrl);

        // Assert — 204, alt hos A er borte
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var stateA = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();
        stateA!.Ingredients.Should().BeEmpty();
        stateA.Meals.Should().BeEmpty();
        stateA.LogEntries.Should().BeEmpty();
        stateA.Goals.Should().BeEmpty();

        // ... og B er urørt
        AuthenticateAs(userB);
        var stateB = await (await _client.GetAsync(StateUrl)).Content.ReadFromJsonAsync<SyncStateResponse>();
        stateB!.Ingredients.Should().ContainSingle().Which.Name.Should().Be("Overlever");
    }

    [Fact]
    public async Task DeleteData_WhenNothingToDelete_IsIdempotent()
    {
        // Arrange — helt fersk bruker uten data
        AuthenticateAs("delete-empty-user");

        // Act — to slettinger på rad
        var first = await _client.DeleteAsync(DataUrl);
        var second = await _client.DeleteAsync(DataUrl);

        // Assert — begge er suksess: å slette ingenting er utført, ikke feil
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
