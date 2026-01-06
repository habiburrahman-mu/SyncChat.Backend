using System.Diagnostics.CodeAnalysis;

namespace SyncChat.API.Shared.Configuration;

[ExcludeFromCodeCoverage(
    Justification = "This is a configuration class and does not require coverage."
)]
public sealed class GoogleAuthSettings
{
    public string ClientId { get; set; } = null!;
}
