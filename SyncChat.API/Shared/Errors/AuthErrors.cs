using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class AuthErrors
{
    public static Error Unauthorized() => Error.Unauthorized(
        "Auth.Unauthorized",
        "You are not authorized to perform this action.");

    public static Error InvalidCredentials => Error.Unauthorized(
        "Auth.InvalidCredentials",
        "The provided credentials are invalid.");
}
