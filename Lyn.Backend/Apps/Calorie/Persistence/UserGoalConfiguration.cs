using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        builder.HasKey(g => g.Id);

        // 450 = Identity sin standardlengde for AspNetUsers.Id
        builder.Property(g => g.UserId).HasMaxLength(450).IsRequired();

        // Oppslaget er alltid "siste mål for bruker per dato" — og maks én
        // målendring per bruker per dag (ny endring samme dag = oppdater raden)
        builder.HasIndex(g => new { g.UserId, g.EffectiveFromDate }).IsUnique();
    }
}
