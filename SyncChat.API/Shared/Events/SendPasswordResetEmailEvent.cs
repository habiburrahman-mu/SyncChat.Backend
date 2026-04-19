namespace SyncChat.API.Shared.Events;

public sealed class SendPasswordResetEmailEvent : IDomainEvent
{
    public string To { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public Guid PasswordResetTokenId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
