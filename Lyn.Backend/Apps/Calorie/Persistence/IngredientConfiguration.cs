using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);

        // 450 = Identity sin standardlengde for AspNetUsers.Id
        builder.Property(i => i.UserId).HasMaxLength(450).IsRequired();
        builder.Property(i => i.Name).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Brand).HasMaxLength(100);
        builder.Property(i => i.UnitName).HasMaxLength(20);
        builder.Property(i => i.UnitWeightGrams).HasPrecision(7, 2);

        builder.OwnsOne(i => i.Nutrition, n => NutritionPer100gConfiguration.Configure(n));
        builder.OwnsOne(i => i.Image, img => StoredImageConfiguration.Configure(img));

        // Løs kobling til felleskatalogen — biblioteket overlever at produktet slettes
        builder.HasOne(i => i.FoodProduct)
            .WithMany()
            .HasForeignKey(i => i.FoodProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // Alle oppslag skjer per bruker (biblioteket er personlig)
        builder.HasIndex(i => i.UserId);
    }
}
