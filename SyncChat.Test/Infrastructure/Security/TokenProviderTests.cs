using FluentAssertions;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.Test.Infrastructure.Security.Fixtures;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(Justification = "This is a test class and does not require coverage.")]
public class TokenProviderTests : IClassFixture<TokenProviderTestFixture>
{
    private readonly TokenProvider _tokenProvider;
    private readonly TokenProviderTestFixture _fixture;

    public TokenProviderTests(TokenProviderTestFixture fixture)
    {
        _fixture = fixture;

        IOptions<JWTSettings> options = Options.Create(_fixture.JwtSettings);

        _tokenProvider = new TokenProvider(options);
    }

    [Fact(DisplayName = "GenerateToken should return a non-empty string token")]
    public void GenerateToken_ShouldReturnNonEmptyToken_WhenUserIsValid()
    {
        // Arrange
        User user = _fixture.TestUser;

        // Act
        string token = _tokenProvider.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Generated token should contain expected claims")]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        // Arrange 
        User user = _fixture.TestUser;

        // Act
        string token = _tokenProvider.GenerateAccessToken(user);
        JwtSecurityToken jwtToken = _fixture.JwtSecurityTokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserID.ToString());
        jwtToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact(DisplayName = "Generate token should have correct issuer and audience")]
    public void GenerateToken_ShouldHaveCorrectIssuerAndAudience()
    {
        // Arrange 
        User user = _fixture.TestUser;

        // Act
        string token = _tokenProvider.GenerateAccessToken(user);
        JwtSecurityToken jwtToken = _fixture.JwtSecurityTokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be(_fixture.JwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_fixture.JwtSettings.Audience);
    }

    [Fact(DisplayName = "Generated token should expire after configured duration")]
    public void GenerateToken_ShouldHaveCorrectExpirationTime()
    {
        // Arrange
        DateTime beforeGeneration = DateTime.UtcNow;
        User user = _fixture.TestUser;
        int expirationInMinutes = _fixture.JwtSettings.AccessTokenExpirationInMinutes;

        // Act
        string token = _tokenProvider.GenerateAccessToken(user);
        JwtSecurityToken jwtToken = _fixture.JwtSecurityTokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(expirationInMinutes - 1));
        jwtToken.ValidFrom.Should().BeBefore(beforeGeneration.AddMinutes(expirationInMinutes + 1));
    }

    [Fact(DisplayName = "GenerateRefreshToken should return a non-empty string token")]
    public void GenerateRefreshToken_ShouldReturnNonEmptyToken()
    {
        // Act
        string token = _tokenProvider.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "GenerateRefreshToken should return unique tokens on multiple calls")]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens_OnMultipleCalls()
    {
        // Act
        string token1 = _tokenProvider.GenerateRefreshToken();
        string token2 = _tokenProvider.GenerateRefreshToken();
        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact(DisplayName = "GenerateRefreshToken should return base64 string")]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var token = _tokenProvider.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        // Validate Base64
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact(DisplayName = "GenerateRefreshToken should generate unique tokens")]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = _tokenProvider.GenerateRefreshToken();
        var token2 = _tokenProvider.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact(DisplayName = "HashRefreshToken with same input should produce same hash")]
    public void HashRefreshToken_SameInput_ShouldProduceSameHash()
    {
        // Arrange
        var refreshToken = "sample-refresh-token";

        // Act
        var hash1 = _tokenProvider.HashRefreshToken(refreshToken);
        var hash2 = _tokenProvider.HashRefreshToken(refreshToken);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
