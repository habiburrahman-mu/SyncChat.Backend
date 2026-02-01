using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public sealed class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("Media");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.OwnerType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.OwnerId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.SizeBytes)
            .IsRequired();

        builder.Property(m => m.State)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        builder.HasMany(m => m.UploadSessions)
           .WithOne(us => us.Media)
           .HasForeignKey(us => us.MediaId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.References)
           .WithOne(r => r.Media)
           .HasForeignKey(r => r.MediaId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.UserId, m.State });
        builder.HasIndex(m => new { m.OwnerType, m.OwnerId });
        builder.HasIndex(m => m.StorageKey).IsUnique();
    }
}
