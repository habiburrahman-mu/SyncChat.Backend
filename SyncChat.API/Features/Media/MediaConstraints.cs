namespace SyncChat.API.Features.Media;

public static class MediaConstraints
{
    public static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        "image/bmp",
        "image/tiff"
    };

    public const long MaxImageFileSizeBytes = 5 * 1024 * 1024; // 5 MB
}
