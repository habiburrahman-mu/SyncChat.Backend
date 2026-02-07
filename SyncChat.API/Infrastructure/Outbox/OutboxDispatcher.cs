
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Events;
using System.Text.Json;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class OutboxDispatcher : BackgroundService
{
    private static readonly TimeSpan ClaimTimeout = TimeSpan.FromMinutes(2);
    private static readonly Guid WorkerId = Guid.NewGuid();

    private readonly ILogger<OutboxDispatcher> logger;
    private readonly ApplicationDbContext dbContext;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public OutboxDispatcher(
        ILogger<OutboxDispatcher> logger,
        ApplicationDbContext dbContext,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Outbox dispatch on {WorkerId:N} failed with an exception. Will retry after delay.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        IServiceProvider provider = scope.ServiceProvider;
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        List<OutboxMessage> claimedMessages = await ClaimBatchAsync(dbContext, 20, cancellationToken);

        if (!claimedMessages.Any()) return;

        foreach (OutboxMessage message in claimedMessages)
        {
            try
            {
                // TODO: need to think about what to do about retry count
                await DispatchMessageAsync(message, provider, cancellationToken);

                message.ProcessedAt = DateTimeOffset.UtcNow;
                message.ClaimedBy = null;
                message.ClaimedAt = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.ClaimedBy = null;
                message.ClaimedAt = null;

                logger.LogError(ex, $"Failed processing outbox message {message.Id} on {WorkerId:N}", message.Id);

                throw;
            }
        }
    }

    private async Task<List<OutboxMessage>> ClaimBatchAsync(
        ApplicationDbContext dbContext,
        int batchSize, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var messages = await dbContext.OutboxMessages
            .FromSqlRaw(@"
                SELECT TOP ({0}) *
                FROM OutboxMessages WITH (UPDLOCK, READPAST)
                WHERE ProcessedAt IS NULL
                  AND (ClaimedAt IS NULL OR ClaimedAt < {1})
                ORDER BY OccurredAt
            ", batchSize, now - ClaimTimeout)
            .ToListAsync(cancellationToken);

        messages.ForEach(m =>
        {
            m.ClaimedBy = WorkerId.ToString("N");
            m.ClaimedAt = now;
        });

        dbContext.OutboxMessages.UpdateRange(messages);

        await dbContext.SaveChangesAsync(cancellationToken);

        return messages;
    }

    private async Task DispatchMessageAsync(
        OutboxMessage message,
        IServiceProvider provider, CancellationToken cancellationToken)
    {
        Type? type = Type.GetType(message.Type);

        if (type is null) return;

        IDomainEvent? domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, type);

        if (domainEvent is null) return;

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = provider.GetServices(handlerType);

        foreach(var handler in handlers)
        {
            await ((dynamic)handler!).HandleAsync((dynamic)domainEvent, cancellationToken); // TODO: no dynamic
        }
    }
}
