namespace SyncChat.API.Shared.Events;

public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
