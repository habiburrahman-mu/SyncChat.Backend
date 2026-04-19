namespace SyncChat.API.Shared.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive(DateTimeOffset now) => UsedAt == null && ExpiresAt > now;
}
