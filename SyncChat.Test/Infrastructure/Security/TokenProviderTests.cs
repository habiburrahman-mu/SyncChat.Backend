using FluentAssertions;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;

namespace SyncChat.Test.Infrastructure.Security;

public class TokenProviderTests
{
    private readonly TokenProvider _tokenProvider;
    private readonly JWTSettings _jwtSettings;

    public TokenProviderTests()
    {
        _jwtSettings = new JWTSettings
        {
            Secret = "SuperSecretKeyForTesting1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        };

        _tokenProvider = new TokenProvider(Options.Create(_jwtSettings));
    }

    [Fact(DisplayName = "GenerateToken should return a non-empty string token")]
    public void GenerateToken_ShouldReturnNonEmptyToken_WhenUserIsValid()
    {
        // Arrange
        var user = new User
        {
            UUID = Guid.NewGuid(),
            Email = "user@example.com"
        };

        // Act
        string token = _tokenProvider.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }
}
