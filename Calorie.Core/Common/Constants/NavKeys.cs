namespace Calorie.Core.Common.Constants;

/// <summary>
/// Nøkler for Shell-navigasjonsparametere — den delte kontrakten mellom
/// avsender- og mottakerside. Aldri rå strenger i kallene.
/// </summary>
public static class NavKeys
{
    /// <summary>Inn til logge-flyten: forhåndsvalgt måltidstype.</summary>
    public const string MealType = "MealType";

    /// <summary>Inn til logge-flyten: dagen det logges til (visningsdatoen) — utelatt = i dag.</summary>
    public const string LogDate = "LogDate";

    /// <summary>Inn til editorene: forhåndsutfylt navn (søketeksten).</summary>
    public const string InitialName = "InitialName";

    /// <summary>Resultat fra editorene: id-en til varen som ble opprettet.</summary>
    public const string CreatedItemId = "CreatedItemId";

    /// <summary>Resultat fra editorene: om det ble måltid eller ingrediens.</summary>
    public const string CreatedItemKind = "CreatedItemKind";

    /// <summary>Inn til ingrediens-editoren: eksisterende ingrediens som skal redigeres.</summary>
    public const string IngredientToEdit = "IngredientToEdit";

    /// <summary>Inn til måltidsbyggeren: eksisterende måltid som skal redigeres.</summary>
    public const string MealToEdit = "MealToEdit";

    /// <summary>Inn til måltidsbyggeren: måltid som skal tilpasses (engangs-kopi).</summary>
    public const string MealToCustomize = "MealToCustomize";

    /// <summary>Resultat fra Tilpass: den tilpassede kopien.</summary>
    public const string MealCustomized = "MealCustomized";
}