using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SyncChat.API.Shared.Configuration;
using static SyncChat.API.Shared.Constants.StorageConstants;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class StorageInitializationService : IHostedService
{
    private readonly IMinioClient minioClient;
    private readonly StorageSettings storageSettings;
    private readonly ILogger<StorageInitializationService> logger;

    public StorageInitializationService(
        [FromKeyedServices(MinioClientKeys.Internal)] IMinioClient minioClient,
        IOptions<StorageSettings> options,
        ILogger<StorageInitializationService> logger)
    {
        this.minioClient = minioClient;
        this.storageSettings = options.Value;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        bool exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(storageSettings.Bucket),
            cancellationToken);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(storageSettings.Bucket),
                cancellationToken);

            logger.LogInformation("Created storage bucket '{Bucket}'.", storageSettings.Bucket);
        }

        string policy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [
                {
                  "Effect": "Allow",
                  "Principal": {"AWS": "*"},
                  "Action": ["s3:GetObject"],
                  "Resource": ["arn:aws:s3:::{{storageSettings.Bucket}}/{{AvatarKeys.Prefix}}*"]
                }
              ]
            }
            """;

        await minioClient.SetPolicyAsync(
            new SetPolicyArgs()
                .WithBucket(storageSettings.Bucket)
                .WithPolicy(policy),
            cancellationToken);

        logger.LogInformation(
            "Applied public-read policy for '{Prefix}' prefix in bucket '{Bucket}'.",
            AvatarKeys.Prefix,
            storageSettings.Bucket);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
