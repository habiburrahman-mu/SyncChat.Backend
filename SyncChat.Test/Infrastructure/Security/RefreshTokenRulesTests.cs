using FluentAssertions;
using SyncChat.API.Shared.Auth;
using SyncChat.API.Shared.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(Justification = "This is a test class and does not require coverage.")]
public class RefreshTokenRulesTests
{
    #region Rotate Tests - New Token Creation

    [Fact(DisplayName = "Rotate should create new token with correct properties when no existing token")]
    public void Rotate_ShouldCreateNewToken_WithCorrectProperties_WhenNoExistingToken()
    {
        // Arrange
        long userId = 123;
        string newTokenHash = "new-token-hash-12345";
        string deviceIdentifier = "device-123";
        DateTime now = DateTime.UtcNow;
        DateTime expiresAt = now.AddDays(7);

        // Act
        RefreshToken result = RefreshTokenRules.Rotate(
            existing: null,
            userId: userId,
            newTokenHash: newTokenHash,
            deviceIdentifier: deviceIdentifier,
            now: now,
            expiresAt: expiresAt);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.TokenHash.Should().Be(newTokenHash);
        result.DeviceIdentifier.Should().Be(deviceIdentifier);
        result.CreatedAt.Should().Be(now);
        result.ExpiresAt.Should().Be(expiresAt);
        result.RevokedAt.Should().BeNull();
    }

    [Fact(DisplayName = "Rotate should generate unique ID for new token")]
    public void Rotate_ShouldGenerateUniqueId_ForNewToken()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token1 = RefreshTokenRules.Rotate(null, 1, "hash1", "device1", now, expiresAt);
        var token2 = RefreshTokenRules.Rotate(null, 2, "hash2", "device2", now, expiresAt);

        // Assert
        token1.Id.Should().NotBe(token2.Id);
        token1.Id.Should().NotBeEmpty();
        token2.Id.Should().NotBeEmpty();
    }

    #endregion

    #region Rotate Tests - Existing Token Revocation

    [Fact(DisplayName = "Rotate should revoke existing token when provided")]
    public void Rotate_ShouldRevokeExistingToken_WhenProvided()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        DateTime expiresAt = now.AddDays(7);
        
        RefreshToken existingToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = 100,
            TokenHash = "old-token-hash",
            DeviceIdentifier = "device-old",
            CreatedAt = now.AddDays(-7),
            ExpiresAt = now.AddDays(7),
            RevokedAt = null
        };

        // Act
        RefreshToken newToken = RefreshTokenRules.Rotate(
            existing: existingToken,
            userId: 100,
            newTokenHash: "new-token-hash",
            deviceIdentifier: "device-new",
            now: now,
            expiresAt: expiresAt);

        // Assert
        existingToken.RevokedAt.Should().NotBeNull();
        existingToken.RevokedAt.Should().Be(now);
    }

    [Fact(DisplayName = "Rotate should create new token even when existing token is revoked")]
    public void Rotate_ShouldCreateNewToken_EvenWhenExistingTokenIsRevoked()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        DateTime expiresAt = now.AddDays(7);
        
        RefreshToken existingToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = 100,
            TokenHash = "old-token-hash",
            DeviceIdentifier = "device-123",
            CreatedAt = now.AddDays(-14),
            ExpiresAt = now.AddDays(-7),
            RevokedAt = now.AddDays(-7) // Already revoked
        };

        // Act
        RefreshToken newToken = RefreshTokenRules.Rotate(
            existing: existingToken,
            userId: 100,
            newTokenHash: "new-token-hash",
            deviceIdentifier: "device-123",
            now: now,
            expiresAt: expiresAt);

        // Assert
        newToken.Should().NotBeNull();
        newToken.Id.Should().NotBe(existingToken.Id);
        newToken.RevokedAt.Should().BeNull();
    }

    #endregion

    #region Rotate Tests - Token Properties

    [Theory(DisplayName = "Rotate should handle different user IDs")]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    [InlineData(long.MaxValue)]
    public void Rotate_ShouldHandleDifferentUserIds(long userId)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token = RefreshTokenRules.Rotate(null, userId, "hash", "device", now, expiresAt);

        // Assert
        token.UserId.Should().Be(userId);
    }

    [Theory(DisplayName = "Rotate should handle different device identifiers")]
    [InlineData("iPhone-12-Pro")]
    [InlineData("Android-Samsung-S21")]
    [InlineData("Web-Chrome-Windows")]
    [InlineData("Desktop-App-MacOS")]
    public void Rotate_ShouldHandleDifferentDeviceIdentifiers(string deviceIdentifier)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, "hash", deviceIdentifier, now, expiresAt);

        // Assert
        token.DeviceIdentifier.Should().Be(deviceIdentifier);
    }

    [Theory(DisplayName = "Rotate should handle different expiration periods")]
    [InlineData(1)]      // 1 day
    [InlineData(7)]      // 1 week
    [InlineData(30)]     // 1 month
    [InlineData(90)]     // 3 months
    public void Rotate_ShouldHandleDifferentExpirationPeriods(int daysToExpire)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(daysToExpire);

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, "hash", "device", now, expiresAt);

        // Assert
        token.ExpiresAt.Should().Be(expiresAt);
        (token.ExpiresAt - token.CreatedAt).Should().BeCloseTo(TimeSpan.FromDays(daysToExpire), TimeSpan.FromMilliseconds(1));
    }

    [Fact(DisplayName = "Rotate should handle long token hashes")]
    public void Rotate_ShouldHandleLongTokenHashes()
    {
        // Arrange
        string longHash = new string('x', 500); // Very long hash
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, longHash, "device", now, expiresAt);

        // Assert
        token.TokenHash.Should().Be(longHash);
        token.TokenHash.Length.Should().Be(500);
    }

    #endregion

    #region Rotate Tests - Token Rotation Scenarios

    [Fact(DisplayName = "Rotate should maintain user ID when rotating token")]
    public void Rotate_ShouldMaintainUserId_WhenRotatingToken()
    {
        // Arrange
        long userId = 999;
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);
        
        RefreshToken existingToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "old-hash",
            DeviceIdentifier = "device-123",
            CreatedAt = now.AddDays(-7),
            ExpiresAt = now.AddDays(7),
            RevokedAt = null
        };

        // Act
        var newToken = RefreshTokenRules.Rotate(
            existingToken, userId, "new-hash", "device-123", now, expiresAt);

        // Assert
        newToken.UserId.Should().Be(userId);
        existingToken.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "Rotate should allow device identifier change during rotation")]
    public void Rotate_ShouldAllowDeviceIdentifierChange_DuringRotation()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);
        
        RefreshToken existingToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = 100,
            TokenHash = "old-hash",
            DeviceIdentifier = "old-device",
            CreatedAt = now.AddDays(-7),
            ExpiresAt = now.AddDays(7),
            RevokedAt = null
        };

        // Act
        var newToken = RefreshTokenRules.Rotate(
            existingToken, 100, "new-hash", "new-device", now, expiresAt);

        // Assert
        newToken.DeviceIdentifier.Should().Be("new-device");
        existingToken.DeviceIdentifier.Should().Be("old-device");
    }

    [Fact(DisplayName = "Rotate should create independent token from existing token")]
    public void Rotate_ShouldCreateIndependentToken_FromExistingToken()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);
        
        RefreshToken existingToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = 100,
            TokenHash = "old-hash",
            DeviceIdentifier = "device-123",
            CreatedAt = now.AddDays(-7),
            ExpiresAt = now.AddDays(7),
            RevokedAt = null
        };

        var existingId = existingToken.Id;
        var existingHash = existingToken.TokenHash;
        var existingCreatedAt = existingToken.CreatedAt;

        // Act
        var newToken = RefreshTokenRules.Rotate(
            existingToken, 100, "new-hash", "device-123", now, expiresAt);

        // Assert - New token has different properties
        newToken.Id.Should().NotBe(existingId);
        newToken.TokenHash.Should().Be("new-hash");
        newToken.CreatedAt.Should().Be(now);
        newToken.RevokedAt.Should().BeNull();

        // Assert - Existing token was modified (revoked)
        existingToken.RevokedAt.Should().Be(now);
        
        // Assert - Existing token keeps its original properties
        existingToken.Id.Should().Be(existingId);
        existingToken.TokenHash.Should().Be(existingHash);
        existingToken.CreatedAt.Should().Be(existingCreatedAt);
    }

    #endregion

    #region Edge Cases

    [Fact(DisplayName = "Rotate should handle expiration time in the past")]
    public void Rotate_ShouldHandleExpirationTimeInThePast()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(-1); // Expires in the past

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, "hash", "device", now, expiresAt);

        // Assert
        token.ExpiresAt.Should().Be(expiresAt);
        token.ExpiresAt.Should().BeBefore(token.CreatedAt);
    }

    [Fact(DisplayName = "Rotate should handle simultaneous creation and expiration")]
    public void Rotate_ShouldHandleSimultaneousCreationAndExpiration()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now; // Expires immediately

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, "hash", "device", now, expiresAt);

        // Assert
        token.CreatedAt.Should().Be(token.ExpiresAt);
    }

    [Fact(DisplayName = "Rotate should handle empty device identifier")]
    public void Rotate_ShouldHandleEmptyDeviceIdentifier()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, "hash", string.Empty, now, expiresAt);

        // Assert
        token.DeviceIdentifier.Should().BeEmpty();
    }

    [Fact(DisplayName = "Rotate should handle empty token hash")]
    public void Rotate_ShouldHandleEmptyTokenHash()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(7);

        // Act
        var token = RefreshTokenRules.Rotate(null, 1, string.Empty, "device", now, expiresAt);

        // Assert
        token.TokenHash.Should().BeEmpty();
    }

    #endregion
}
