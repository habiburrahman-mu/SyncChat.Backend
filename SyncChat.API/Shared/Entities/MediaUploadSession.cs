namespace SyncChat.API.Shared.Entities;

public class MediaUploadSession
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }
    public Media Media { get; set; } = default!;

    // Upload control
    public DateTimeOffset ExpiresAt { get; set; }
    public int MaxUploads { get; set; }
    public int UploadCount { get; set; }

    // Audit
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
