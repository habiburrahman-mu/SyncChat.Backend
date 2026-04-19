using System.Text.Json;

namespace SyncChat.API.Shared.Entities;

public class User
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
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; } 
    public string? PasswordHash { get; set; }
    public JsonDocument Profile { get; set; }
    public UserStatus Status { get; set; }
    public DateTimeOffset? LastActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBanned { get; set; }
    public string? AvatarKey { get; set; }

    // Navigation collections
    public ICollection<Conversation> CreatedConversations { get; set; } = new List<Conversation>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public ICollection<UserAuthProvider> UserAuthProviders { get; set; } = new List<UserAuthProvider>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}

public enum UserStatus
{
    Offline,
    Online,
    Away,
    Busy
    // Add other statuses if needed
}