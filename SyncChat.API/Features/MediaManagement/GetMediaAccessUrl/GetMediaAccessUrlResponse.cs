namespace SyncChat.API.Features.MediaManagement.GetMediaAccessUrl;

public sealed record GetMediaAccessUrlResponse(
    Guid MediaId,
    string Url,
    DateTimeOffset ExpiresAt);
