namespace Lyn.Backend.Apps.Calorie.Models;

/// <summary>
/// Referanse til et opplastet bilde i S3. Delt value object (owned type)
/// for alt som kan ha ett valgfritt bilde (Ingredient, Meal). Backend
/// lagrer kun nøkkelen — aldri selve filen.
/// </summary>
public class StoredImage
{
    public string StorageKey { get; set; } = string.Empty;

    // Nedskalert versjon til lister/grid — genereres ved opplasting.
    // Invariant: finnes bildet, finnes thumbnailen. Feiler genereringen,
    // feiler hele opplastingen (ingenting lagres).
    public string ThumbnailKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
