using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class ConversationMemberErrors
{
    public static Error NotFound(long memberId) => Error.NotFound(
        "ConversationMember.NotFound",
        $"Entry not found for Id = {memberId}.");

    public static Error Forbidden() => Error.Forbidden(
        "ConversationMember.Forbidden",
        $"You do not have necessary permission to perform this action.'");
}
