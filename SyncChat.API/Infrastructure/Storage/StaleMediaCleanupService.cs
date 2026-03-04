using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Infrastructure.Storage;

public sealed class StaleMediaCleanupService(
    ILogger<StaleMediaCleanupService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan StaleInitiatedThreshold = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(RunInterval);

        do
        {
            try
            {
                await CleanupStaleInitiatedAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stale media cleanup failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
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
}
