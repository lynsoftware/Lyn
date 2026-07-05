namespace Lyn.Backend.Apps.Calorie.Enums;

/// <summary>
/// Hvilket måltid på dagen en LogEntry gjelder. Brukes til gruppering av
/// dagsloggen. Klienten foreslår automatisk (første logg på dagen = frokost
/// osv.), brukeren kan alltid overstyre.
/// </summary>
public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,

    // Mellommåltid — planlagt småmat mellom hovedmåltidene (frukt, yoghurt)
    Snack = 3,
    Brunch = 4,

    // Kveldsmat — eget måltid i norsk matkultur, ikke det samme som snack
    Supper = 5,

    // Kos — godteri/utskeielser, typisk med eget dagsbudsjett på tvers av måltidene
    Treat = 6
}
