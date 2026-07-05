using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.UserId).HasMaxLength(450).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();

        // Løs referanse til malen — loggen overlever at malen slettes
        builder.HasOne(l => l.Meal)
            .WithMany()
            .HasForeignKey(l => l.MealId)
            .OnDelete(DeleteBehavior.SetNull);

        // Snapshot-radene eies av loggen og slettes med den
        builder.HasMany(l => l.Ingredients)
            .WithOne(i => i.LogEntry)
            .HasForeignKey(i => i.LogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dashboard og dagslogg spør alltid "denne brukeren, denne dagen" —
        // på brukerens lokale dato, ikke UTC-tidspunktet
        builder.HasIndex(l => new { l.UserId, l.LoggedDate });
    }
}
