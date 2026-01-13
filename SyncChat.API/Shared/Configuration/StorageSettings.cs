namespace SyncChat.API.Shared.Configuration;

public sealed class StorageSettings
{
    public string Endpoint { get; init; } = default!;
    public int Port { get; init; }
    public string AccessKey { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public string Bucket { get; init; } = default!;
    public bool UseSSL { get; init; }
}
