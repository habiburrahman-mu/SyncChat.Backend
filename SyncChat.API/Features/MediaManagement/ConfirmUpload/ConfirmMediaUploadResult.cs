using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.MediaManagement.ConfirmUpload;

public sealed record ConfirmMediaUploadResult(Guid MediaId, MediaState MediaState);
