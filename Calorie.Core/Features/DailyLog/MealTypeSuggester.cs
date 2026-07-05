using Calorie.Core.Common;

namespace Calorie.Core.Features.DailyLog;

/// <summary>
/// Foreslår måltidstype ut fra klokkeslettet — brukeren kan alltid overstyre.
/// </summary>
public static class MealTypeSuggester
{
    public static MealType SuggestFor(TimeOnly time) => time.Hour switch
    {
        < 10 => MealType.Breakfast,
        < 14 => MealType.Lunch,
        < 17 => MealType.Snack,
        < 21 => MealType.Dinner,
        _ => MealType.Supper
    };
}
