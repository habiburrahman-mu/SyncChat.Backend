using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Events;
using System.Text.Json;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class OutboxImmediateEventPublisher : IDomainEventPublisher
{
    private readonly ApplicationDbContext dbContext;
    private readonly DomainEventChannel domainEventChannel;
    private readonly ILogger<OutboxImmediateEventPublisher> logger;
    private readonly List<IDomainEvent> pendingEvents = [];

    public OutboxImmediateEventPublisher(
        ApplicationDbContext dbContext,
        DomainEventChannel domainEventChannel,
        ILogger<OutboxImmediateEventPublisher> logger)
    {
        this.dbContext = dbContext;
        this.domainEventChannel = domainEventChannel;
        this.logger = logger;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        OutboxMessage message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredAt = domainEvent.OccurredAt,
            RetryCount = 0
        };

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        pendingEvents.Add(domainEvent);
    }

    public void DispatchPendingEvents()
    {
        foreach (IDomainEvent domainEvent in pendingEvents)
        {
            if (!domainEventChannel.Writer.TryWrite(domainEvent))
            {
                logger.LogWarning(
                    "Failed to write {EventType} to the immediate channel. Outbox will handle it.",
                    domainEvent.GetType().Name);
            }
        }

        pendingEvents.Clear();
    }
}
