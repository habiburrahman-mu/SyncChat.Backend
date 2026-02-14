namespace SyncChat.API.Shared.Entities;

public class Media
{
    public Guid Id { get; set; }

    // Uploader / owner
    public Guid UserId { get; set; }

    // Logical ownership (not FK)
    public MediaOwnerType OwnerType { get; set; } = default!;
    public string OwnerId { get; set; } = default!;

    // File metadata
    public string MimeType { get; set; } = default!;
    public long SizeBytes { get; set; }

    // Storage
    public string StorageKey { get; set; } = default!;

    // Lifecycle
    public MediaState State { get; set; } = default!;

    // Timestamps
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public ICollection<MediaUploadSession> UploadSessions { get; set; } = new List<MediaUploadSession>();
    public ICollection<MediaReference> References { get; set; } = new List<MediaReference>();
}

public enum MediaState
{
    Unknown = 0,

    Initiated = 1,   // DB row created, upload not completed
    Uploaded = 2,    // Binary exists in storage
    Active = 3,      // Active Media
    Attached = 4,    // At least one MediaReference exists

    Deleting = 8,    // Async delete in progress
    Deleted = 9,     // Tombstone (optional)
    Failed = 10      // Upload or processing failed
}

public enum MediaOwnerType
{
    Unknown = 0,

    User = 1,
    Message = 2,
    Conversation = 3,

    System = 100
}