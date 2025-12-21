using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Security.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SyncChat.API.Infrastructure.Security;

public sealed class TokenProvider(IOptions<JWTSettings> jwtSettings) : ITokenProvider
{
    private readonly JWTSettings _jwtSettings = jwtSettings.Value;

    public string GenerateAccessToken(User user)
    {
        string secretKey = _jwtSettings.Secret;
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        JwtSecurityToken tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationInMinutes),
            signingCredentials: credentials
        );

        JwtSecurityTokenHandler jwtSecurityToken = new JwtSecurityTokenHandler();

        string token = jwtSecurityToken.WriteToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
