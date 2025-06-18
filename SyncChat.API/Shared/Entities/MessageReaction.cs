namespace SyncChat.API.Shared.Entities;

public class MessageReaction
{
    public long ReactionId { get; set; }
    public long MessageId { get; set; }
    public long UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Message Message { get; set; } = default!;
    public User User { get; set; } = default!;
}
