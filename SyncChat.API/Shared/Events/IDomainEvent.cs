namespace SyncChat.API.Shared.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
