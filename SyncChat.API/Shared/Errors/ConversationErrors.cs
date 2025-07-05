using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class ConversationErrors
{
    public static Error InvalidUser(long userId) => Error.Validation(
        "Conversation.InvalidUser",
        $"The user with the Id = '{userId}' is invalid.");
}
