using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Storage.Contracts;
using SyncChat.API.Shared.Storage.Contracts.Models;
using static SyncChat.API.Shared.Constants.StorageConstants;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class MinioBlobStorage : IBlobStorage
{
    private readonly IMinioClient minioClient;
    private readonly IMinioClient presignClient;
    private readonly StorageSettings storageSettings;

    public MinioBlobStorage(
        [FromKeyedServices(MinioClientKeys.Internal)] IMinioClient minioClient,
        [FromKeyedServices(MinioClientKeys.Presign)] IMinioClient presignClient,
        IOptions<StorageSettings> options)
    {
        this.minioClient = minioClient;
        this.presignClient = presignClient;
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

    public async Task<BlobUploadResult> UploadAsync(Stream fileStream, string contentType, string objectName, CancellationToken cancellationToken)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(storageSettings.Bucket),
            cancellationToken);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(storageSettings.Bucket),
                cancellationToken);
        }

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.CanSeek ? fileStream.Length : -1)
            .WithContentType(contentType);

        var putObjectResponse = await minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        return new BlobUploadResult
        {
            ObjectName = objectName,
            Bucket = storageSettings.Bucket,
            ContentType = contentType,
            ETag = putObjectResponse.Etag,
            SizeInBytes = putObjectResponse.Size,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        RemoveObjectArgs removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName);

        await minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    public async Task<string> GeneratePresignedUploadUrlAsync(
        string objectName,
        TimeSpan validFor,
        CancellationToken cancellationToken)
    {
        var presignedPutObjectArgs = new PresignedPutObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName)
            .WithExpiry((int)validFor.TotalSeconds);

        return await presignClient.PresignedPutObjectAsync(presignedPutObjectArgs);
    }

    public async Task<string> GeneratePresignedDownloadUrlAsync(
        string objectName,
        TimeSpan validFor,
        CancellationToken cancellationToken)
    {
        var exists = await minioClient.StatObjectAsync(
            new StatObjectArgs()
                .WithBucket(storageSettings.Bucket)
                .WithObject(objectName),
            cancellationToken);

        if (exists is null)
            throw new FileNotFoundException("Object not found in storage.");

        var presignedGetObjectArgs = new PresignedGetObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName)
            .WithExpiry((int)validFor.TotalSeconds);

        return await presignClient.PresignedGetObjectAsync(presignedGetObjectArgs);
    }

    public async Task<BlobMetadata?> GetMetadataAsync(string objectName, CancellationToken cancellationToken)
    {
        StatObjectArgs statObjectArgs = new StatObjectArgs()
            .WithBucket(storageSettings.Bucket)
            .WithObject(objectName);

        ObjectStat? stat = await minioClient.StatObjectAsync(statObjectArgs, cancellationToken);

        if (stat is null) return null;

        return new BlobMetadata(
            ObjectName: stat.ObjectName,
            ContentType: stat.ContentType ?? "application/octet-stream",
            Size: stat.Size,
            ETag: stat.ETag
        );
    }

    public async IAsyncEnumerable<string> ListObjectKeysAsync(
        string prefix,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ListObjectsArgs args = new ListObjectsArgs()
            .WithBucket(storageSettings.Bucket)
            .WithPrefix(prefix)
            .WithRecursive(true);

        await foreach (Item item in minioClient.ListObjectsEnumAsync(args, cancellationToken))
        {
            if (!item.IsDir)
                yield return item.Key;
        }
    }
}
