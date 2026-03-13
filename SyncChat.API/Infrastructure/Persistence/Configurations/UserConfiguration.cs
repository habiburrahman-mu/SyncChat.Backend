using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.UserID);

        // Properties
        builder.Property(u => u.UserID)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UUID)
               .IsRequired();

        builder.Property(u => u.UserName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(u => u.Phone)
               .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
               .IsRequired(false);

        builder.Property(u => u.Profile)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(u => u.Status)
               // Postgres enum type - registered in DbContext
               .HasColumnType("user_status")
               .IsRequired();

        builder.Property(u => u.LastActive)
               .IsRequired(false);

        builder.Property(u => u.CreatedAt)
               .IsRequired();

        builder.Property(u => u.UpdatedAt)
               .IsRequired();

        builder.Property(u => u.IsVerified)
               .IsRequired();

        builder.Property(u => u.IsBanned)
               .IsRequired();

        builder.Property(u => u.AvatarKey)
               .HasMaxLength(500)
               .IsRequired(false);

        // Optional: Add indexes if needed
        //builder.HasIndex(u => u.Email)
        //       .IsUnique();

        //builder.HasIndex(u => u.UserName)
        //       .IsUnique();
    }
}
