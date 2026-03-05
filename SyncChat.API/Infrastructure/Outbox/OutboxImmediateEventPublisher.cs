using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Events;
using System.Text.Json;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class OutboxImmediateEventPublisher : IDomainEventPublisher
{
    private readonly ApplicationDbContext dbContext;
    private readonly DomainEventChannel domainEventChannel;
    private readonly ILogger<OutboxImmediateEventPublisher> logger;
    private readonly List<DomainEventEnvelope> pendingEvents = [];

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
        Guid messageId = Guid.NewGuid();

        OutboxMessage message = new OutboxMessage
        {
            Id = messageId,
            Type = domainEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredAt = domainEvent.OccurredAt,
            RetryCount = 0
        };

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        pendingEvents.Add(new DomainEventEnvelope(messageId, domainEvent));
    }

    public void DispatchPendingEvents()
    {
        foreach (DomainEventEnvelope envelope in pendingEvents)
        {
            if (!domainEventChannel.Writer.TryWrite(envelope))
            {
                logger.LogWarning(
                    "Failed to write {EventType} to the immediate channel. Outbox will handle it.",
                    envelope.DomainEvent.GetType().Name);
            }
        }

        pendingEvents.Clear();
    }
}
