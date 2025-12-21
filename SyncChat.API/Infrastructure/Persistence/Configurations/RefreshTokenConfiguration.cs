using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        // Primary Key
        builder.HasKey(rt => rt.Id);

        // Properties
        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(rt => rt.DeviceIdentifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .IsRequired(false);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // for faster look ups
        builder
            .HasIndex(rt => new { rt.UserId, rt.DeviceIdentifier });

        // Refresh tokens must be globally unique
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        // for cleaning up expired tokens
        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
