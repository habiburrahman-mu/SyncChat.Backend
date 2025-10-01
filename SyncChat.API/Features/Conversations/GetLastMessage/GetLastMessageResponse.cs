namespace SyncChat.API.Features.Conversations.GetLastMessage;

public sealed record GetLastMessageResponse(
    long LastMessageId,
    string? Content,
    string? MetaData);
