using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

namespace SyncChat.Test.Infrastructure.Security.Fixtures;

[ExcludeFromCodeCoverage(Justification = "This is a test class fixture and does not require coverage.")]
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
            AccessTokenExpirationInMinutes = 60
        };

        TestUser = new User
        {
            UUID = Guid.NewGuid(),
            Email = "user@example.com"
        };

        JwtSecurityTokenHandler = new();
    }
}
