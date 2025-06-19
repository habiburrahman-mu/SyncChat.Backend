using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.Uuid).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.MetaData).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.Property(x => x.Type).HasConversion<string>();

        builder.Property(x => x.SearchVector).HasColumnType("tsvector");

        builder.HasOne(m => m.Conversation)
               .WithMany(c => c.Messages)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.ParentMessage)
               .WithMany()
               .HasForeignKey(m => m.ReplyTo)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Sender)
               .WithMany(u => u.SentMessages)
               .HasForeignKey(m => m.SenderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
