using FluentAssertions;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(
    Justification = "This is a test class and does not require coverage."
)]
public class PasswordResetTokenServiceTests
{
    private readonly PasswordResetTokenService _service;
    private readonly JWTSettings _jwtSettings;

    public PasswordResetTokenServiceTests()
    {
        _jwtSettings = new JWTSettings
        {
            AccessTokenSecret = "b9w84#9dk@dkfj0293JSDK@39dkf#kd02kd9Wke",
            RefreshTokenSecret = "f9w84@9dk#dkfja393BYjk@ABcdf#kd02kd9qSA",
            Issuer = "SyncChat",
            Audience = "SyncChatClient",
            AccessTokenExpirationInMinutes = 10,
            RefreshTokenExpirationInMinutes = 10080,
            PasswordResetUrlBase = "http://localhost:4200/password-reset",
            PasswordResetExpirationInMinutes = 60,
            PasswordResetTokenSecret = "a-very-strong-secret-that-is-long-enough-for-hmac"
        };

        var options = Options.Create(_jwtSettings);
        _service = new PasswordResetTokenService(options);
    }

    #region CreateToken Tests

    [Fact(DisplayName = "CreateToken should return a token string with dot separator")]
    public void CreateToken_ShouldReturnTokenWithDotSeparator()
    {
        // Arrange
        var tokenId = Guid.NewGuid();

        // Act
        var token = _service.CreateToken(tokenId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
        var parts = token.Split('.');
        parts.Length.Should().Be(2);
    }

    [Fact(DisplayName = "CreateToken should produce different tokens for different token IDs")]
    public void CreateToken_ShouldProduceDifferentTokens_ForDifferentTokenIds()
    {
        // Arrange
        var tokenId1 = Guid.NewGuid();
        var tokenId2 = Guid.NewGuid();

        // Act
        var token1 = _service.CreateToken(tokenId1);
        var token2 = _service.CreateToken(tokenId2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact(DisplayName = "CreateToken should produce same token for same token ID")]
    public void CreateToken_ShouldProduceSameToken_ForSameTokenId()
    {
        // Arrange
        var tokenId = Guid.NewGuid();

        // Act
        var token1 = _service.CreateToken(tokenId);
        var token2 = _service.CreateToken(tokenId);

        // Assert
        token1.Should().Be(token2);
    }

    #endregion

    #region TryParseToken Tests

    [Fact(DisplayName = "TryParseToken should successfully parse valid token")]
    public void TryParseToken_ShouldSuccessfullyParse_ValidToken()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var token = _service.CreateToken(originalId);

        // Act
        var result = _service.TryParseToken(token, out var parsedId);

        // Assert
        result.Should().BeTrue();
        parsedId.Should().Be(originalId);
    }

    [Theory(DisplayName = "TryParseToken should fail for invalid tokens")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-token")]
    [InlineData("invalid.token.with.more.parts")]
    public void TryParseToken_ShouldFail_ForInvalidTokens(string token)
    {
        // Act
        var result = _service.TryParseToken(token, out var parsedId);

        // Assert
        result.Should().BeFalse();
        parsedId.Should().Be(Guid.Empty);
    }

    [Fact(DisplayName = "TryParseToken should fail for tampered token")]
    public void TryParseToken_ShouldFail_ForTamperedToken()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var token = _service.CreateToken(originalId);
        var parts = token.Split('.');
        var tamperedToken = parts[0] + ".tampered_signature";

        // Act
        var result = _service.TryParseToken(tamperedToken, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "TryParseToken should fail for token with invalid GUID")]
    public void TryParseToken_ShouldFail_ForInvalidGuid()
    {
        // Arrange
        var token = "invalid-guid.signature";

        // Act
        var result = _service.TryParseToken(token, out _);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Configuration Tests

    [Fact(DisplayName = "Constructor should throw when PasswordResetTokenSecret is too short")]
    public void Constructor_ShouldThrow_WhenSecretIsTooShort()
    {
        // Arrange
        var jwtSettings = new JWTSettings
        {
            PasswordResetTokenSecret = "short" // Less than 16 characters
        };
        var options = Options.Create(jwtSettings);

        // Act
        Action act = () => new PasswordResetTokenService(options);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*must be at least 16 characters*");
    }

    [Fact(DisplayName = "Constructor should throw when PasswordResetTokenSecret is null or empty")]
    public void Constructor_ShouldThrow_WhenSecretIsNullOrEmpty()
    {
        // Arrange
        var jwtSettings = new JWTSettings
        {
            PasswordResetTokenSecret = null!
        };
        var options = Options.Create(jwtSettings);

        // Act
        Action act = () => new PasswordResetTokenService(options);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>();
    }

    #endregion
}
