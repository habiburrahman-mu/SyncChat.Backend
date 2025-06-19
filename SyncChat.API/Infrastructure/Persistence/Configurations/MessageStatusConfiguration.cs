using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class MessageStatusConfiguration : IEntityTypeConfiguration<MessageStatus>
{
    public void Configure(EntityTypeBuilder<MessageStatus> builder)
    {
        builder.HasKey(x => x.StatusId);

        builder.Property(x => x.Status).HasConversion<string>();

        builder.HasIndex(x => new { x.MessageId, x.UserId }).IsUnique();

        builder.HasOne(ms => ms.Message)
               .WithMany(m => m.Statuses)
               .HasForeignKey(ms => ms.MessageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ms => ms.User)
               .WithMany()
               .HasForeignKey(ms => ms.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
