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

    public static Error NotFound(Guid mediaId) => Error.NotFound(
        "Media.NotFound",
        $"No media found with ID '{mediaId}'.");

    public static Error UploadSessionNotFound(Guid mediaId) => Error.NotFound(
        "Media.UploadSessionNotFound",
        $"No upload session found for media with ID '{mediaId}'.");

    public static Error UploadSessionExpired(Guid mediaId) => Error.Validation(
        "Media.UploadSessionExpired",
        $"The upload session for media with ID '{mediaId}' has expired.");

    public static Error BlobMetadataNotFound(Guid mediaId) => Error.NotFound(
        "Media.BlobMetadataNotFound",
        $"No blob metadata found for media with ID '{mediaId}'.");

    public static Error InvalidBlobMeta(Guid mediaId) => Error.Validation(
        "Media.InvalidBlobMeta",
        $"The blob metadata for media with ID '{mediaId}' is invalid or incomplete.");

    public static Error NoUploadAttemptDetected(Guid mediaId) => Error.Validation(
        "Media.NoUploadAttemptDetected",
        $"No upload attempt detected for media with ID '{mediaId}'.");

    public static Error MediaIsNotActive => Error.Validation(
        "Media.MediaIsNotActive",
        $"Selected media is not active and not ready to use.");

    public static Error Forbidden => Error.Forbidden(
        "Media.Forbidden",
        $"You do not have necessary permission to perform this action.");
}
