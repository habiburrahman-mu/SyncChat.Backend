using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class MediaCleanupService(
    ILogger<MediaCleanupService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan StaleInitiatedThreshold = TimeSpan.FromHours(2);
    private const string MediaBlobPrefix = "media/";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupStaleInitiatedAsync(stoppingToken);
                await CleanupOrphanedBlobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Media cleanup failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task CleanupStaleInitiatedAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blobStorage = scope.ServiceProvider.GetRequiredService<IBlobStorage>();

        DateTimeOffset threshold = DateTimeOffset.UtcNow.Subtract(StaleInitiatedThreshold);

        List<Media> staleMedia = await dbContext.Media
            .Where(m => m.State == MediaState.Initiated && m.CreatedAt < threshold)
            .ToListAsync(cancellationToken);

        if (staleMedia.Count == 0) return;

        foreach (Media media in staleMedia)
        {
            try
            {
                await blobStorage.DeleteAsync(media.StorageKey, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not delete blob for stale media {MediaId}. Row will still be removed.", media.Id);
            }
        }

        dbContext.Media.RemoveRange(staleMedia);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Removed {Count} stale initiated media records.", staleMedia.Count);
    }

    private async Task CleanupOrphanedBlobsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var blobStorage = scope.ServiceProvider.GetRequiredService<IBlobStorage>();

        IReadOnlyList<string> blobKeys = await blobStorage.ListObjectKeysAsync(MediaBlobPrefix, cancellationToken);

        if (blobKeys.Count == 0) return;

        HashSet<string> knownKeys = (await dbContext.Media
            .AsNoTracking()
            .Where(m => blobKeys.Contains(m.StorageKey))
            .Select(m => m.StorageKey)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        int deletedCount = 0;

        foreach (string key in blobKeys)
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

        if (deletedCount > 0)
            logger.LogInformation("Deleted {Count} orphaned blobs.", deletedCount);
    }
}
