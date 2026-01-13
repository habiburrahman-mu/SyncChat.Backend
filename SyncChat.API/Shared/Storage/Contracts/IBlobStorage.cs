using SyncChat.API.Shared.Storage.Contracts.Models;

namespace SyncChat.API.Shared.Storage.Contracts;

public interface IBlobStorage
{
    Task<BlobUploadResult> UploadAsync(
        Stream fileStream,
        string contentType,
        string objectName,
        CancellationToken cancellationToken);

    Task<BlobDownloadResult> DownloadAsync(
        string objectName,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string objectName,
        CancellationToken cancellationToken);
}
