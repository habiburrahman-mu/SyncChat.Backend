using System.Diagnostics.CodeAnalysis;

namespace SyncChat.API.Shared.Configuration;

[ExcludeFromCodeCoverage(
    Justification = "This is a configuration class and does not require coverage."
)]
public class JWTSettings
{
    public string AccessTokenSecret { get; set; } = null!;
    public string RefreshTokenSecret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInMinutes { get; set; }
    public string PasswordResetUrlBase { get; set; } = null!;
    public int PasswordResetExpirationInMinutes { get; set; }
    public string PasswordResetTokenSecret { get; set; } = null!;
}
