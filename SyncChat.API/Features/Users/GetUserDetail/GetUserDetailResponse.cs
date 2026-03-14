using SyncChat.API.Shared.Entities;
using System.Text.Json;

namespace SyncChat.API.Features.Users.GetUserDetail;

public sealed record GetUserDetailResponse(
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
    bool IsVerified,
    bool IsBanned,
    string? AvatarUrl);