using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class ConversationMemberConfiguration : IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> builder)
    {
        builder.HasKey(cm => cm.MemberId);

        builder.Property(x => x.Settings)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Property(x => x.Role)
            .HasConversion<string>();

        builder.HasIndex(cm => new { cm.ConversationId, cm.UserId })
            .IsUnique();

        builder.HasOne(cm => cm.Conversation)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.LastSeenMessage)
            .WithMany()
            .HasForeignKey(cm => cm.LastSeenMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
