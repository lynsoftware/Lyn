using Lyn.Backend.Platform.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Platform.Auth.Persistence;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.PreferredCulture)
            .HasMaxLength(10)
            .HasDefaultValue("en");
    }
}