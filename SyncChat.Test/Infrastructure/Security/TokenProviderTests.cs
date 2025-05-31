using FluentAssertions;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.Test.Infrastructure.Security.Fixtures;

namespace SyncChat.Test.Infrastructure.Security;

public class TokenProviderTests: IClassFixture<TokenProviderTestFixture>
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
}
