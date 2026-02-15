using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record MessageNotificationModel(
    long MessageId,
    Guid Uuid,
    long ConversationId,
    long SenderId,
    string SenderUserName,
    string SenderName,
    MessageType Type,
    string? Content,
    Guid? MediaId,
    string MetaData,
    long? ReplyTo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
