using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found.");

    public static Error NotFound(long userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found.");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found.");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "Registration already exists with the provided email.");

    public static readonly Error UserNameNotUnique = Error.Conflict(
        "Users.UserNameNotUnique",
        "The provided user name is not unique.");

    public static readonly Error InvalidUserNamePassword = Error.Validation(
        "Users.Invalid.Credentials",
        "The username or password you entered is incorrect.");

    public static readonly Error InvalidUserName = Error.Validation(
         "Users.Invalid.UserNameRequest",
         "Invalid user name.");

    public static readonly Error InvalidDeviceId = Error.Validation(
         "Users.Invalid.DeviceIdentifier",
         "Invalid device identifier.");

    public static readonly Error InvalidRefreshToken = Error.Validation(
         "Users.Invalid.RefreshToken",
         "Invalid refresh token.");

    public static Error UserNameNotFound(string userName) => Error.NotFound(
       "Users.NotFound",
       $"The user with the user name = '{userName}' was not found.");
}
