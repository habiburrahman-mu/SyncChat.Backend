using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Conversations.DTOs;

public class ConversationDTO
{
    public long ConversationId { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? OtherUserAvatarKey { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? LastMessageId { get; set; }
    public string Settings { get; set; } = "{}";
    public string? LastMessage { get; set; } = string.Empty;
    public string? LastMessageMetaData { get; set; } = string.Empty;
    public MessageType? LastMessageType { get; set; }
    public long? OtherUserId { get; set; }
    public long? LastSeenMessageId { get; set; }
    public bool HaveUnreadMessages { get; set; }
}

public static class ConversationExtensions
{
    public static ConversationDTO? ToDTO(this Conversation? conversation)
    {
        if (conversation == null)
            return null;

        var dto = new ConversationDTO
        {
            ConversationId = conversation.ConversationId,
            Uuid = conversation.Uuid,
            Type = conversation.Type,
            Name = conversation.Name ?? string.Empty,
            AvatarUrl = conversation.AvatarUrl,
            CreatedBy = conversation.CreatedBy,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageId = conversation.LastMessageId,
            Settings = conversation.Settings
        };

        if(conversation.LastMessage != null)
        {
            dto.LastMessage = conversation.LastMessage.Content;
            dto.LastMessageMetaData = conversation.LastMessage.MetaData;
            dto.LastMessageType = conversation.LastMessage.Type;
        }

        return dto;
    }

    public static List<ConversationDTO> ToDTOs(this List<Conversation> conversations)
    {
        return conversations
                .Select(c => c.ToDTO())
                .Where(dto => dto != null)
                .Cast<ConversationDTO>()
                .ToList();
    }
}