using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).HasMaxLength(450).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Brand).HasMaxLength(100);
        builder.Property(m => m.Brand).HasMaxLength(100);

        builder.OwnsOne(m => m.Image, img => StoredImageConfiguration.Configure(img));

        // Sletting av malen fjerner koblingsradene — men aldri loggene
        builder.HasMany(m => m.Ingredients)
            .WithOne(mi => mi.Meal)
            .HasForeignKey(mi => mi.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.UserId);
    }
}
