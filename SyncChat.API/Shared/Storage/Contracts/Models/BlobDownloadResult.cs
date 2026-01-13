namespace SyncChat.API.Shared.Storage.Contracts.Models;

public sealed record BlobDownloadResult
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
    public required long SizeInBytes { get; init; }
    public string? ETag { get; init; }
}