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
        string token = _tokenProvider.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Generated token should contain expected claims")]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        // Arrange 
        User user = _fixture.TestUser;

        // Act
        string token = _tokenProvider.GenerateToken(user);
        JwtSecurityToken jwtToken = _fixture.JwtSecurityTokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UUID.ToString());
        jwtToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact(DisplayName = "Generate token should have correct issuer and audience")]
    public void GenerateToken_ShouldHaveCorrectIssuerAndAudience()
    {
        // Arrange 
        User user = _fixture.TestUser;

        // Act
        string token = _tokenProvider.GenerateToken(user);
        JwtSecurityToken jwtToken = _fixture.JwtSecurityTokenHandler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be(_fixture.JwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_fixture.JwtSettings.Audience);
    }
}
