namespace SyncChat.API.Shared.Entities;

public class MediaReference
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }
    public Media Media { get; set; } = default!;

    // Logical reference (not FK)
    public MediaRefType RefType { get; set; } = default!;
    public string RefId { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}

public enum MediaRefType
{
    Unknown = 0,

    Message = 1,
    UserAvatar = 2,
    UserThumbnail = 3,
    ConversationAvatar = 4,

    // future-safe expansion
    System = 100
}
