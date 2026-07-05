using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class LogEntryIngredientConfiguration : IEntityTypeConfiguration<LogEntryIngredient>
{
    public void Configure(EntityTypeBuilder<LogEntryIngredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IngredientName).HasMaxLength(100).IsRequired();
        builder.Property(i => i.AmountGrams).HasPrecision(7, 2);

        builder.OwnsOne(i => i.Nutrition, n => NutritionPer100gConfiguration.Configure(n));
    }
}
