using Lyn.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Platform.Support.Persistence;

internal sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasKey(t => t.Id);

        // Én ticket har mange vedlegg. Sletting av en ticket kaskaderer til vedleggene.
        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.SupportTicket)
            .HasForeignKey(a => a.SupportTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lengder og required-regler kommer fra DataAnnotations på modellen (delt med klientene).
    }
}
