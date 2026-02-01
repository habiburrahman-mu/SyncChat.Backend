using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public sealed class MediaUploadSessionConfiguration : IEntityTypeConfiguration<MediaUploadSession>
{
    public void Configure(EntityTypeBuilder<MediaUploadSession> builder)
    {
        builder.ToTable("MediaUploadSessions");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.MediaId)
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.MaxUploads)
            .IsRequired();

        builder.Property(s => s.UploadCount)
            .IsRequired();

        builder.Property(s => s.UsedAt)
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne(s => s.Media)
            .WithMany(m => m.UploadSessions)
            .HasForeignKey(s => s.MediaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.MediaId); // fast lookup by Media
        builder.HasIndex(s => s.ExpiresAt); // cleanup query
    }
}
