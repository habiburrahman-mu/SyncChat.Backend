using System.Text.Json;

namespace SyncChat.API.Shared.Entities;

public class User : BaseEntity
{
    public User()
    {
        UUID = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        Profile = JsonDocument.Parse("{}");
    }

    public long UserID { get; set; }
    public Guid UUID { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } 
    public string PasswordHash { get; set; } = null!;
    public JsonDocument Profile { get; set; }
    public UserStatus Status { get; set; }
    public DateTimeOffset? LastActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBanned { get; set; }

}

public enum UserStatus
{
    Offline,
    Online,
    Away,
    Busy
    // Add other statuses if needed
}