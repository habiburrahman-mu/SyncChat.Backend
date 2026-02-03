namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record InitiateUploadResponse(
    Guid MediaId,
    Uri UploadUri,
    DateTimeOffset Expiration);