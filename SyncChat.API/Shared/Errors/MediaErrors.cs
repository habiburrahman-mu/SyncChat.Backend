using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Errors;

public static class MediaErrors
{
    public static Error UnsupportedMimeType(string mimeType) => Error.Validation(
        "Media.UnsupportedMimeType",
        $"The MIME type '{mimeType}' is not supported for media uploads.");

    public static Error FileTooLarge(long maxSizeBytes) => Error.Validation(
        "Media.FileTooLarge",
        $"The uploaded file exceeds the maximum allowed size of {maxSizeBytes / (1024 * 1024)} MB.");
}
