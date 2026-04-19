using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Security.Contracts;

namespace SyncChat.API.Infrastructure.Security;

public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    private const char Separator = '.';
    private readonly byte[] secretKey;

    public PasswordResetTokenService(IOptions<JWTSettings> jwtOptions)
    {
        string secret = jwtOptions.Value.PasswordResetTokenSecret
            ?? throw new InvalidOperationException("PasswordResetTokenSecret is not configured in JWT settings.");

        secretKey = Encoding.UTF8.GetBytes(secret);

        if (secretKey.Length < 16)
        {
            throw new InvalidOperationException("PasswordResetTokenSecret must be at least 16 characters.");
        }
    }

    public string CreateToken(Guid resetTokenId)
    {
        string id = resetTokenId.ToString("N");
        string signature = CreateSignature(id);
        return string.Join(Separator, id, signature);
    }

    public bool TryParseToken(string token, out Guid resetTokenId)
    {
        resetTokenId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        string[] parts = token.Split(Separator, 2);
        if (parts.Length != 2)
            return false;

        if (!Guid.TryParseExact(parts[0], "N", out resetTokenId))
            return false;

        string expectedSignature = CreateSignature(parts[0]);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(parts[1]));
    }

    private string CreateSignature(string resetTokenId)
    {
        using var hmac = new HMACSHA256(secretKey);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(resetTokenId));
        return Convert.ToHexString(hash);
    }
}
