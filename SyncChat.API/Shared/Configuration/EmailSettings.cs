using System.Diagnostics.CodeAnalysis;

namespace SyncChat.API.Shared.Configuration;

[ExcludeFromCodeCoverage(
    Justification = "This is a configuration class and does not require coverage."
)]
public class EmailSettings
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = null!;
    public string SmtpPass { get; set; } = null!;
    public string From { get; set; } = null!;
}
