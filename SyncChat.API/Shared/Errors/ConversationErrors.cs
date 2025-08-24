using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class ConversationErrors
{
    public static Error NotFound(long conversationId) => Error.NotFound(
        "Conversation.NotFound",
        $"Conversation not found for conversation Id = {conversationId}.");

    public static Error InvalidUser(long userId) => Error.Validation(
        "Conversation.InvalidUser",
        $"The user with the Id = '{userId}' is invalid.");

    public static Error NotAuthorized(long conversationId) => Error.Validation(
        "Conversation.NotAuthorized",
        $"You are not authorized to perform this action on the conversation with Id = '{conversationId}'.");

    public static Error LastMessageNotFound(long conversationId) => Error.NotFound(
        "Conversation.LastMessageNotFound",
        $"Last message not found for conversation Id = {conversationId}.");
}
