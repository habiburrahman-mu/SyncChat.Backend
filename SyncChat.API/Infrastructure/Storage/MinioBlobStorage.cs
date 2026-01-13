using SyncChat.API.Shared.Storage.Contracts;
using SyncChat.API.Shared.Storage.Contracts.Models;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class MinioBlobStorage : IBlobStorage
{
    public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BlobDownloadResult> DownloadAsync(string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BlobUploadResult> UploadAsync(Stream fileStream, string contentType, string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
