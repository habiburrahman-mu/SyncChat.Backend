namespace SyncChat.API.Shared.Storage.Contracts.Models;

public sealed record BlobMetadata(
    string ObjectName,
    string ContentType,
    long Size,
    string ETag);
