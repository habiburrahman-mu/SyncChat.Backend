using Microsoft.Extensions.Options;
using Minio;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Storage.Contracts;
using SyncChat.API.Shared.Storage.Contracts.Models;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class MinioBlobStorage : IBlobStorage
{
    private readonly IMinioClient minioClient;
    private readonly StorageSettings storageSettings;

    public MinioBlobStorage(IMinioClient minioClient, IOptions<StorageSettings> options)
    {
        this.minioClient = minioClient;
        this.storageSettings = options.Value;
    }

    public Task<BlobDownloadResult> DownloadAsync(string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BlobUploadResult> UploadAsync(Stream fileStream, string contentType, string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
