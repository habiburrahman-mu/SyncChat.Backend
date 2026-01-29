namespace SyncChat.API.Shared.Storage.Contracts.Models;

public sealed record BlobObjectInfo
{
    public required string ObjectName { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public string? ETag { get; init; }
}