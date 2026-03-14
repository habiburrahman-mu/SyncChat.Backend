namespace SyncChat.API.Features.Users.GetUserByUserName;

public sealed record GetUserByUserNameResponse(
    long UserID,
    Guid UUID,
    string UserName,
    string Name,
    string? AvatarUrl);