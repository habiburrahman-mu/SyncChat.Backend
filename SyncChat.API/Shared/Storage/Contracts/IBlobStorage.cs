using SyncChat.API.Shared.Storage.Contracts.Models;

namespace SyncChat.API.Shared.Storage.Contracts;

public interface IBlobStorage
{
    Task<BlobObjectInfo> GetInfoAsync(string objectName, CancellationToken cancellationToken);

    Task<BlobUploadResult> UploadAsync(
        Stream fileStream,
        string contentType,
        string objectName,
        CancellationToken cancellationToken);

    Task DownloadAsync(
        string objectName,
        Func<Stream, CancellationToken, Task> writeTo,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string objectName,
        CancellationToken cancellationToken);

    Task<string> GeneratePresignedUploadUrlAsync(
        string objectName,
        TimeSpan validFor,
        CancellationToken cancellationToken);

    Task<BlobMetadata?> GetMetadataAsync(
        string objectName, 
        CancellationToken cancellationToken);
}
