using SyncChat.API.Shared.Entities;
using System.Text.Json;

namespace SyncChat.API.Features.Users.UpdateUser;

public sealed record UpdateUserResponse(
    long UserID,
    Guid UUID,
    string UserName,
    string Name,
    string Email,
    string? Phone,
    JsonDocument Profile,
    UserStatus Status,
    DateTimeOffset? LastActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool IsVerified,
    bool IsBanned);