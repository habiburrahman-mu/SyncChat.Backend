using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
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

    public async Task<BlobObjectInfo> GetInfoAsync(string objectName, CancellationToken cancellationToken)
    {
        var stat = await minioClient.StatObjectAsync(new StatObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName), cancellationToken);

        return new BlobObjectInfo
        {
            ObjectName = stat.ObjectName,
            ContentType = stat.ContentType ?? "application/octet-stream",
            Size = stat.Size,
            ETag = stat.ETag
        };
    }

    public async Task DownloadAsync(
        string objectName,
        Func<Stream, CancellationToken, Task> writeTo,
        CancellationToken cancellationToken)
    {
        GetObjectArgs getObjectArgs = new GetObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName)
            .WithCallbackStream(writeTo);

        await minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
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
