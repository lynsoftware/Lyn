using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;

namespace Calorie.Common.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoBackAsync() => Shell.Current.GoToAsync((".."));

    public Task GoBackAsync(string resultKey, object resultValue) =>
        Shell.Current.GoToAsync("..", new Dictionary<string, object> { [resultKey] = resultValue });

    public Task GoToIngredientEditorAsync(string? initialName = null) =>
        Shell.Current.GoToAsync(AppRoutes.IngredientEditor, WithOptional(NavKeys.InitialName, initialName));

    public Task GoToMealBuilderAsync(string? initialName = null) =>
        Shell.Current.GoToAsync(AppRoutes.MealBuilder, WithOptional(NavKeys.InitialName, initialName));

    public Task GoToCustomizeMealAsync(LibraryMeal meal) =>
        Shell.Current.GoToAsync(AppRoutes.MealBuilder,
            new Dictionary<string, object> { [NavKeys.MealToCustomize] = meal });

    public Task GoToEditIngredientAsync(LibraryIngredient ingredient) =>
        Shell.Current.GoToAsync(AppRoutes.IngredientEditor,
            new Dictionary<string, object> { [NavKeys.IngredientToEdit] = ingredient });

    public Task GoToEditMealAsync(LibraryMeal meal) =>
        Shell.Current.GoToAsync(AppRoutes.MealBuilder,
            new Dictionary<string, object> { [NavKeys.MealToEdit] = meal });

    public Task GoToAddLogEntryAsync(MealType? mealType = null, DateOnly? date = null)
    {
        var parameters = new Dictionary<string, object>();
        if (mealType is { } type)
            parameters[NavKeys.MealType] = type;
        if (date is { } logDate)
            parameters[NavKeys.LogDate] = logDate;

        return Shell.Current.GoToAsync(AppRoutes.AddLogEntry, parameters);
    }

    public Task GoHomeAsync() => Shell.Current.Navigation.PopToRootAsync();

    public Task GoToLibraryAsync() => Shell.Current.GoToAsync(AppRoutes.Library);

    public Task GoToStatsAsync() => Shell.Current.GoToAsync(AppRoutes.Stats);

    public Task GoToSettingsAsync() => Shell.Current.GoToAsync(AppRoutes.Settings);

    public Task GoToGoalSettingsAsync() => Shell.Current.GoToAsync(AppRoutes.GoalSettings);

    // Utelater nøkkelen helt når verdien mangler — mottaker slipper null-sjekk på selve oppslaget
    private static Dictionary<string, object> WithOptional(string key, object? value) =>
        value == null ? new Dictionary<string, object>() : new Dictionary<string, object> { [key] = value };
}