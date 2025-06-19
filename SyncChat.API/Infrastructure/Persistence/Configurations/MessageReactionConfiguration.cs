using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.HasKey(x => x.ReactionId);

        builder.HasIndex(x => new { x.MessageId, x.UserId, x.Emoji }).IsUnique();

        builder.HasOne(mr => mr.Message)
               .WithMany(m => m.Reactions)
               .HasForeignKey(mr => mr.MessageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.User)
               .WithMany()
               .HasForeignKey(mr => mr.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
