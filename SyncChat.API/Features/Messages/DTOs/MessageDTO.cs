using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Messages.DTOs;

public class MessageDTO
{
    public long MessageId { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public MessageType Type { get; set; }
    public string? Content { get; set; }
    public Guid? MediaId { get; set; }
    public string MetaData { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
    public long? ReplyTo { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTimeOffset? EditedAt { get; set; }
    public string SenderUserName { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string? SenderAvatarUrl { get; set; }
}

public static class MessageExtensions
{
    public static MessageDTO ToDTO(this Message message, Func<string, string>? publicUrlResolver = null)
    {
        return new MessageDTO
        {
            MessageId = message.MessageId,
            Uuid = message.Uuid,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Type = message.Type,
            Content = message.Content,
            MediaId = message.MediaId,
            MetaData = message.MetaData,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            DeletedAt = message.DeletedAt,
            ReplyTo = message.ReplyTo,
            IsEdited = message.IsEdited,
            EditedAt = message.EditedAt,
            SenderName = message.Sender?.Name ?? string.Empty,
            SenderUserName = message.Sender?.UserName ?? string.Empty,
            SenderAvatarUrl = message.Sender?.AvatarKey is string key ? publicUrlResolver?.Invoke(key) : null
        };
    }
}