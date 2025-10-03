using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Conversations.GetLastMessage;

public sealed record GetLastMessageResponse(
    long LastMessageId,
    string? Content,
    MessageType? MessageType,
    string? MetaData);
