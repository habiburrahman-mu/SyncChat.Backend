namespace SyncChat.API.Shared.Configuration;

public sealed class StorageSettings
{
    public string Endpoint { get; init; } = default!;
    public int Port { get; init; }
    public string AccessKey { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public string Bucket { get; init; } = default!;
    public bool UseSSL { get; init; }

    /// <summary>
    /// The public-facing base URL for presigned URLs that the browser/client will use.
    /// When set, presigned URLs will have their internal host replaced with this value.
    /// Example: "http://localhost:9000"
    /// </summary>
    public string? PublicUrl { get; init; }
}
