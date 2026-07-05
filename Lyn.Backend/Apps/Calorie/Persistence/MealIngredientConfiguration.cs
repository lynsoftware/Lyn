using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class MealIngredientConfiguration : IEntityTypeConfiguration<MealIngredient>
{
    public void Configure(EntityTypeBuilder<MealIngredient> builder)
    {
        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.AmountGrams).HasPrecision(7, 2);

        // En ingrediens som er i bruk i en mal kan ikke slettes —
        // brukeren må fjerne den fra måltidene først
        builder.HasOne(mi => mi.Ingredient)
            .WithMany()
            .HasForeignKey(mi => mi.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Samme ingrediens kun én gang per måltid — juster mengden i stedet
        builder.HasIndex(mi => new { mi.MealId, mi.IngredientId }).IsUnique();
    }
}
