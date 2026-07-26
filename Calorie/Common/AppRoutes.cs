namespace Calorie.Common;

/// <summary>
/// Shell-rutenavn — registreres i MauiProgram og brukes kun av
/// navigasjonstjenesten. Sider og ViewModels kjenner dem ikke.
/// </summary>
public static class AppRoutes
{
    public const string AddLogEntry = "addlogentry";
    public const string IngredientEditor = "ingredienteditor";
    public const string MealBuilder = "mealbuilder";
    public const string Library = "library";
    public const string Stats = "stats";
    public const string Settings = "settings";
    public const string GoalSettings = "goalsettings";
}