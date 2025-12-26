using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Shared.Auth;

public static class RefreshTokenRules
{
    public static RefreshToken Rotate(
        RefreshToken? existing,
        long userId,
        string newTokenHash,
        string deviceIdentifier,
        DateTime now,
        DateTime expiresAt)
    {
        existing?.Revoke(now);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = newTokenHash,
            DeviceIdentifier = deviceIdentifier,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            RevokedAt = null
        };
    }
}
