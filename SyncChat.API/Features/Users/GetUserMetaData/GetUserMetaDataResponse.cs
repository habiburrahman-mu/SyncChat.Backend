namespace SyncChat.API.Features.Users.GetUserMetaData;

public sealed record GetUserMetaDataResponse(
    long UserId,
    string UserName,
    string Name,
    string? AvatarUrl);
