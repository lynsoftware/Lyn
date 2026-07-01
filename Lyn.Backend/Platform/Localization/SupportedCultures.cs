namespace Lyn.Backend.Platform.Localization;

/// <summary>
/// Kanonisk liste over kulturene appen støtter. Én kilde til sannhet som både
/// RequestLocalization-oppsettet og validering av brukerens språkvalg bruker.
/// </summary>
public class SupportedCultures
{
    /// <summary>Fallback-kultur når verken JWT-claim eller Accept-Language gir treff.</summary>
    public const string Default = "en";

    /// <summary>Alle støttede kulturkoder (BCP-47).</summary>
    public static readonly string[] Codes = ["en", "nb"];
    
    /// <summary>
    /// Sjekker om en kultur er støttet
    /// </summary>
    /// <param name="culture">Kulteren som blir sjekket</param>
    /// <returns>True hvis koden er en av de støttede kulturene</returns>
    public static bool IsSupported(string? culture) =>
        !string.IsNullOrWhiteSpace(culture) && Codes.Contains(culture);
}