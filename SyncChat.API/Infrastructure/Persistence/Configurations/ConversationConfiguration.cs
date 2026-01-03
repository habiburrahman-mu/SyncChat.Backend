using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration: IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.ConversationId);

        builder.Property(x => x.Settings).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        
        builder.Property(x => x.Type).HasConversion<string>();

        builder.HasOne(c => c.Creator)
            .WithMany(u => u.CreatedConversations)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.LastMessage)
            .WithMany()
            .HasForeignKey(c => c.LastMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
