using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Conversations.DTOs;

public class ConversationDTO
{
    public long ConversationId { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? LastMessageId { get; set; }
    public string Settings { get; set; } = "{}";
    public string? LastMessage { get; set; } = string.Empty;
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

        return new ConversationDTO
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
    }

    public static List<ConversationDTO> ToDTOs(this List<Conversation> conversations)
    {
        return conversations
                .Select(c =>
                {
                    var dto = c.ToDTO();
                    if (dto != null && c.LastMessage != null)
                    {
                        dto.LastMessage = c.LastMessage.Content;
                    }
                    return dto;
                })
                .Where(dto => dto != null)
                .Cast<ConversationDTO>()
                .ToList();
    }
}