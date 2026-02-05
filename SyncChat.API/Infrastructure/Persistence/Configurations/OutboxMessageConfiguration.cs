using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Outbox;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired().HasMaxLength(255);

        builder.Property(m => m.Payload).IsRequired();

        builder.Property(m => m.OccurredAt).IsRequired();

        builder.Property(m => m.ProcessedAt);

        builder.Property(m => m.ClaimedBy).HasMaxLength(255);

        builder.Property(m => m.ClaimedAt);

        builder.Property(m => m.RetryCount).IsRequired();
    }
}
