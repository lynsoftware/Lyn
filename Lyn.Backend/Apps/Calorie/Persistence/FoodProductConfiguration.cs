using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class FoodProductConfiguration : IEntityTypeConfiguration<FoodProduct>
{
    public void Configure(EntityTypeBuilder<FoodProduct> builder)
    {
        builder.HasKey(p => p.Id);

        // EAN-8/13 (GTIN opptil 14 siffer). Én EAN = ett produkt i katalogen.
        builder.Property(p => p.Ean).HasMaxLength(14).IsRequired();
        builder.HasIndex(p => p.Ean).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Brand).HasMaxLength(100);
        builder.Property(p => p.UnitName).HasMaxLength(20);
        builder.Property(p => p.UnitWeightGrams).HasPrecision(7, 2);

        // 450 = Identity sin standardlengde for AspNetUsers.Id
        builder.Property(p => p.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(p => p.UpdatedByUserId).HasMaxLength(450);

        builder.OwnsOne(p => p.Nutrition, n => NutritionPer100gConfiguration.Configure(n));
        builder.OwnsOne(p => p.Image, img => StoredImageConfiguration.Configure(img));
    }
}
