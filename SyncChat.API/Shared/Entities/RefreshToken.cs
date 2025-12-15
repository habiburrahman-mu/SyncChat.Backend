namespace SyncChat.API.Shared.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public string DeviceIdentifier { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
