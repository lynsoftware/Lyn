using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

/// <summary>
/// Felles kolonne-mapping for StoredImage (owned type). Brukes av både
/// Ingredient og Meal slik at nye bildefelter (f.eks. thumbnail) konfigureres
/// ett sted. Kolonnene blir nullable siden eieren er valgfri (Image kan være null).
/// </summary>
internal static class StoredImageConfiguration
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, StoredImage> image)
        where TOwner : class
    {
        image.Property(i => i.StorageKey).HasColumnName("ImageStorageKey").HasMaxLength(512);
        image.Property(i => i.ThumbnailKey).HasColumnName("ImageThumbnailKey").HasMaxLength(512);
        image.Property(i => i.ContentType).HasColumnName("ImageContentType").HasMaxLength(100);
        image.Property(i => i.UploadedAtUtc).HasColumnName("ImageUploadedAtUtc");
    }
}
