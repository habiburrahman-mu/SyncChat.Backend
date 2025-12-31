
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;

namespace SyncChat.API.Infrastructure.Security;

public class RefreshTokenCleanupService(
    ILogger<RefreshTokenCleanupService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // runs once per day

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cleaning up refresh tokens.");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DateTime now = DateTime.UtcNow;

        var deletedCount = await dbContext.RefreshTokens
            .Where(rt => rt.RevokedAt != null || rt.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            logger.LogInformation("Cleaned up {Count} expired or revoked refresh tokens.", deletedCount);
        }
    }
}
