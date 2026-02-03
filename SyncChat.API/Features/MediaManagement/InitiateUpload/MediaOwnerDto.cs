using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.MediaManagement.InitiateUpload;

public sealed record MediaOwnerDto(MediaOwnerType Type, string Id);
