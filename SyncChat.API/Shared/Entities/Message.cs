namespace SyncChat.API.Shared.Entities;

public class Message
{
    public long MessageId { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public MessageType Type { get; set; }
    public string? Content { get; set; }
    public string MetaData { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
    public long? ReplyTo { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTimeOffset? EditedAt { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = default!;
    public Message? ParentMessage { get; set; }
    public User Sender { get; set; } = default!;
    public ICollection<MessageStatus> Statuses { get; set; } = new List<MessageStatus>();
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
}

public enum MessageType { Text, Image, Video, File }