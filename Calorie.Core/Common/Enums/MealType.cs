namespace Calorie.Core.Common.Enums;

/// <summary>
/// Måltidstype for en loggført spising. MÅ speile MealType-enumen i
/// AFBackend (Lyn.Backend.Apps.Calorie.Enums.MealType) — verdiene er
/// del av sync-kontrakten.
/// </summary>
public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,

    // Mellommåltid — planlagt småmat mellom hovedmåltidene
    Snack = 3,
    Brunch = 4,

    // Kveldsmat
    Supper = 5,

    // Kos — godteri/utskeielser med eget dagsbudsjett
    Treat = 6
}
