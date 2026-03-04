using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class OrphanBlobCleanupService(
    ILogger<OrphanBlobCleanupService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
    private const string MediaBlobPrefix = "media/";
    private const int BatchSize = 200;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOrphanedBlobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Orphan blob cleanup failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task CleanupOrphanedBlobsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blobStorage = scope.ServiceProvider.GetRequiredService<IBlobStorage>();

        int deletedCount = 0;
        List<string> batch = new(BatchSize);

        await foreach (string key in blobStorage.ListObjectKeysAsync(MediaBlobPrefix, cancellationToken))
        {
            batch.Add(key);

            if (batch.Count < BatchSize) continue;

            deletedCount += await DeleteOrphanBatchAsync(dbContext, blobStorage, batch, cancellationToken);
            batch.Clear();
        }

        if (batch.Count > 0)
            deletedCount += await DeleteOrphanBatchAsync(dbContext, blobStorage, batch, cancellationToken);

        if (deletedCount > 0)
            logger.LogInformation("Deleted {Count} orphaned blobs.", deletedCount);
    }

    private async Task<int> DeleteOrphanBatchAsync(
        ApplicationDbContext dbContext,
        IBlobStorage blobStorage,
        List<string> batch,
        CancellationToken cancellationToken)
    {
        HashSet<string> knownKeys = (await dbContext.Media
            .AsNoTracking()
            .Where(m => batch.Contains(m.StorageKey))
            .Select(m => m.StorageKey)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        int deletedCount = 0;

        foreach (string key in batch)
        {
            if (knownKeys.Contains(key)) continue;

            try
            {
                await blobStorage.DeleteAsync(key, cancellationToken);
                deletedCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not delete orphaned blob {Key}.", key);
            }
        }

        return deletedCount;
    }
}
