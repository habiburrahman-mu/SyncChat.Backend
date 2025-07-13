using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Messages.SendMessage;

public sealed record SendMessageResponse(
    long MessageId,
    Guid Uuid,
    long ConversationId,
    long SenderId,
    MessageType Type,
    string? Content,
    string SenderUserName,
    string SenderByName,
    string MetaData = "{}",
    long? ReplyTo = null);