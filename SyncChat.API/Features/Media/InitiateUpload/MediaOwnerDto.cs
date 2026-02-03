using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record MediaOwnerDto(MediaOwnerType Type, string Id);
