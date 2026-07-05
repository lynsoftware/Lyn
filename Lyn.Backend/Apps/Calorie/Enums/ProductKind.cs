namespace Lyn.Backend.Apps.Calorie.Enums;

/// <summary>
/// Hva et FoodProduct typisk brukes som — styrer kun hva appen FORESLÅR
/// ved skanning (Ingredient: "legg i biblioteket", ReadyMeal: "opprett som
/// måltid"). Brukeren kan alltid overstyre; datamodellen behandler begge likt.
/// </summary>
public enum ProductKind
{
    Ingredient = 0,   // ketchup, mel, kyllingfilet i pakke
    ReadyMeal = 1     // Fjordland, frossenpizza, ferdigsalat
}
