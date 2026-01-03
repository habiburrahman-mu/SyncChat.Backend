using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public sealed class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.ToTable("UserAuthProviders");

        builder.HasKey(uap => uap.Id);

        builder.Property(uap => uap.Provider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(uap => uap.ProviderUserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(uap => uap.Email)
            .HasMaxLength(320);

        // Relationship
        builder.HasOne(uap => uap.User)
             .WithMany(u => u.UserAuthProviders)
             .HasForeignKey(uap => uap.UserID)
             .OnDelete(DeleteBehavior.Cascade);

        // Unique (Provider, ProviderUserId)
        builder.HasIndex(uap => new { uap.Provider, uap.ProviderUserId })
            .IsUnique();

        // Unique (Provider, Email) where Email is not null
        builder.HasIndex(uap => new { uap.Provider, uap.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");
    }
}
