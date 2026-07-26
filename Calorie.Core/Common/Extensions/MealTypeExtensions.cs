using Calorie.Core.Common.Enums;
using Calorie.Core.Resources.Strings;

namespace Calorie.Core.Common.Extensions;

public static class MealTypeExtensions
{
    /// <summary>
    /// Lokalisert visningsnavn for måltidstypen (Snack = mellommåltid, Treat = kos).
    /// </summary>
    public static string ToDisplayName(this MealType type) => type switch
    {
        MealType.Breakfast => AppResources.MealTypeBreakfast,
        MealType.Lunch => AppResources.MealTypeLunch,
        MealType.Dinner => AppResources.MealTypeDinner,
        MealType.Snack => AppResources.MealTypeSnack,
        MealType.Brunch => AppResources.MealTypeBrunch,
        MealType.Supper => AppResources.MealTypeSupper,
        MealType.Treat => AppResources.MealTypeTreat,
        _ => type.ToString()
    };

    /// <summary>
    /// Visningsrekkefølge gjennom dagen — enum-verdiene er sync-kontrakt og
    /// kan ikke renummereres, så rekkefølgen defineres her.
    /// </summary>
    public static int DisplayOrder(this MealType type) => type switch
    {
        MealType.Breakfast => 0,
        MealType.Brunch => 1,
        MealType.Lunch => 2,
        MealType.Snack => 3,
        MealType.Dinner => 4,
        MealType.Supper => 5,
        MealType.Treat => 6,
        _ => 7
    };
}
