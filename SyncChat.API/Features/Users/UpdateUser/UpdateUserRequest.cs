namespace SyncChat.API.Features.Users.UpdateUser;

public sealed record UpdateUserRequest(
    string Name,
    string Email,
    string? Phone);
