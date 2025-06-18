namespace SyncChat.API.Shared.Entities;

public class MessageStatus
{
    public long StatusId { get; set; }
    public long MessageId { get; set; }
    public long UserId { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Sent;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Message Message { get; set; } = default!;
    public User User { get; set; } = default!;
}

public enum DeliveryStatus { Sent, Delivered, Read }