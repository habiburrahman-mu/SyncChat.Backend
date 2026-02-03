namespace SyncChat.API.Features.MediaManagement.InitiateUpload;

public sealed record InitiateUploadResponse(
    Guid MediaId,
    Uri UploadUri,
    DateTimeOffset Expiration);