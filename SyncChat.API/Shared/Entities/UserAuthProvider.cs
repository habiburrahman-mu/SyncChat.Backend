namespace SyncChat.API.Shared.Entities;

public class UserAuthProvider
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public AuthProvider Provider { get; set; }
    public string ProviderUserId { get; set; } = null!;
    public string? Email { get; set; }
    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}

public enum AuthProvider
{
    Local = 1,
    Google = 2,
    Facebook = 3,
    Twitter = 4,
    GitHub = 5,
    Microsoft = 6
}
