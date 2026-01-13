namespace SyncChat.API.Shared.Storage.Contracts.Models;

public sealed record BlobUploadResult
{
    public required string ObjectName { get; init; }
    public required string Bucket { get; init; }
    public required string ContentType { get; init; }
    public required long SizeInBytes { get; init; }
    public required string ETag { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
}
