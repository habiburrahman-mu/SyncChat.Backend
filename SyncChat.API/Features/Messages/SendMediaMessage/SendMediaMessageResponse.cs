using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Messages.SendMediaMessage;

public sealed record SendMediaMessageResponse(
    long MessageId,
    Guid Uuid,
    long ConversationId,
    long SenderId,
    MessageType Type,
    Guid MediaId,
    string? Content,
    string SenderUserName,
    string SenderByName,
    DateTimeOffset UpdatedAt,
    string MetaData = "{}",
    long? ReplyTo = null );
