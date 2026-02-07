using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Events;
using System.Text.Json;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class OutboxEventPublisher : IDomainEventPublisher
{
    private readonly ApplicationDbContext dbContext;

    public OutboxEventPublisher(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Asynchronously publishes a domain event by persisting it to the outbox for later processing.
    /// </summary>
    /// <remarks>This method adds the domain event to the outbox, enabling reliable event delivery and
    /// eventual processing. The event is not dispatched immediately; it is stored for later handling by an outbox
    /// processor. Thread safety depends on the underlying database context implementation. Call save changes to persist the outbox message.</remarks>
    /// <param name="domainEvent">The domain event to be published. Cannot be null. The event's data will be serialized and stored in the outbox.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
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

        await dbContext.OutboxMessages.AddAsync(message);
    }
}
