using Lyn.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lyn.Backend.Platform.Support.Persistence;

internal sealed class SupportAttachmentConfiguration : IEntityTypeConfiguration<SupportAttachment>
{
    public void Configure(EntityTypeBuilder<SupportAttachment> builder)
    {
        builder.HasKey(a => a.Id);

        // Relasjonen til SupportTicket er definert i SupportTicketConfiguration (én side eier den).
        // Lengder og required-regler kommer fra DataAnnotations på modellen (delt med klientene).
    }
}
