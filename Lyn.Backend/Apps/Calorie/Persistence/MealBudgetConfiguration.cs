using Lyn.Backend.Apps.Calorie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Apps.Calorie.Persistence;

internal sealed class MealBudgetConfiguration : IEntityTypeConfiguration<MealBudget>
{
    public void Configure(EntityTypeBuilder<MealBudget> builder)
    {
        builder.HasKey(b => b.Id);

        // Budsjettene eies av målet og slettes med det
        builder.HasOne(b => b.UserGoal)
            .WithMany(g => g.MealBudgets)
            .HasForeignKey(b => b.UserGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Maks ett budsjett per måltidstype per mål
        builder.HasIndex(b => new { b.UserGoalId, b.MealType }).IsUnique();
    }
}
