using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace SyncChat.Test.Infrastructure.Security.Fixtures;

public class TokenProviderTestFixture
{
    public JWTSettings JwtSettings { get; }
    public User TestUser { get; }
    public JwtSecurityTokenHandler JwtSecurityTokenHandler { get; }

    public TokenProviderTestFixture()
    {
        JwtSettings = new JWTSettings
        {
            Secret = "SuperSecretKeyForTesting1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        };

        TestUser = new User
        {
            UUID = Guid.NewGuid(),
            Email = "user@example.com"
        };

        JwtSecurityTokenHandler = new();
    }
}
