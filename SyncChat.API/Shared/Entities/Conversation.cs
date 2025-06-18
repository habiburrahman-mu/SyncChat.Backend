namespace SyncChat.API.Shared.Entities;

public class Conversation
{
    public long ConversationId { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public ConversationType Type { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? LastMessageId { get; set; }
    public string Settings { get; set; } = "{}";
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public User? Creator { get; set; }
    public Message? LastMessage { get; set; }
    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();

}

public enum ConversationType
{
    Direct,
    Group,
    Channel
}
