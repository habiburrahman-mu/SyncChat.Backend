using SyncChat.API.Shared.Events;

namespace SyncChat.API.Features.MediaManagement.MediaUploadedEvent;

public sealed record class MediaUploadedEvent(Guid MediaId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
