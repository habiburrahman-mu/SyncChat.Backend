namespace SyncChat.API.Shared.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
    void DispatchPendingEvents();
}
