using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Shared.Auth;

public static class RefreshTokenRules
{
    public static RefreshToken Rotate(
        RefreshToken? existing,
        long userId,
        string tokenHash,
        string deviceIdentifier,
        DateTime now,
        DateTime expiresAt)
    {
        existing?.Revoke(now);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            DeviceIdentifier = deviceIdentifier,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            RevokedAt = null
        };
    }
}
