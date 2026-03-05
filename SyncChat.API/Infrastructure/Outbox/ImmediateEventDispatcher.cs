using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Events;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class ImmediateEventDispatcher : BackgroundService
{
    private static readonly Guid WorkerId = Guid.NewGuid();

    private readonly DomainEventChannel domainEventChannel;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ImmediateEventDispatcher> logger;

    public ImmediateEventDispatcher(
        DomainEventChannel domainEventChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ImmediateEventDispatcher> logger)
    {
        this.domainEventChannel = domainEventChannel;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (DomainEventEnvelope envelope in domainEventChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = serviceScopeFactory.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;
            ApplicationDbContext dbContext = provider.GetRequiredService<ApplicationDbContext>();

            try
            {
                bool claimed = await TryClaimAsync(dbContext, envelope.OutboxMessageId, stoppingToken);

                if (!claimed) continue;

                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(envelope.DomainEvent.GetType());
                var handlers = provider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    await ((dynamic)handler!).HandleAsync((dynamic)envelope.DomainEvent, stoppingToken);
                }

                await MarkProcessedAsync(dbContext, envelope.OutboxMessageId, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Immediate dispatch failed for {EventType} (OutboxMessageId: {MessageId}). Outbox will retry.",
                    envelope.DomainEvent.GetType().Name, envelope.OutboxMessageId);

                await TryReleaseClaimAsync(dbContext, envelope.OutboxMessageId, stoppingToken);
            }
        }
    }

    private static async Task<bool> TryClaimAsync(
        ApplicationDbContext dbContext, Guid outboxMessageId, CancellationToken cancellationToken)
    {
        int claimed = await dbContext.OutboxMessages
            .Where(m => m.Id == outboxMessageId
                     && m.ProcessedAt == null
                     && m.ClaimedBy == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.ClaimedBy, WorkerId.ToString("N"))
                .SetProperty(m => m.ClaimedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        return claimed > 0;
    }

    private static async Task MarkProcessedAsync(
        ApplicationDbContext dbContext, Guid outboxMessageId, CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages
            .Where(m => m.Id == outboxMessageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.ProcessedAt, DateTimeOffset.UtcNow)
                .SetProperty(m => m.ClaimedBy, (string?)null)
                .SetProperty(m => m.ClaimedAt, (DateTimeOffset?)null),
                cancellationToken);
    }

    private async Task TryReleaseClaimAsync(
        ApplicationDbContext dbContext, Guid outboxMessageId, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.OutboxMessages
                .Where(m => m.Id == outboxMessageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.RetryCount, m => m.RetryCount + 1)
                    .SetProperty(m => m.ClaimedBy, (string?)null)
                    .SetProperty(m => m.ClaimedAt, (DateTimeOffset?)null),
                    cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to release claim for outbox message {MessageId}. Claim timeout will handle it.",
                outboxMessageId);
        }
    }
}
