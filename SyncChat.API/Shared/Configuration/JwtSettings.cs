using System.Diagnostics.CodeAnalysis;

namespace SyncChat.API.Shared.Configuration;

[ExcludeFromCodeCoverage(
    Justification = "This is a configuration class and does not require coverage."
)]
public class JWTSettings
{
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationInMinutes { get; set; }
}
