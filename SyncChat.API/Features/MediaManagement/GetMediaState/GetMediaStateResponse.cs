using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.MediaManagement.GetMediaState;

public sealed record GetMediaStateResponse(
    Guid MediaId,
    int MediaState);
