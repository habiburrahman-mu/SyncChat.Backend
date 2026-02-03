using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record InitiateUploadCommand(
    MediaOwnerDto Owner,
    MediaFileDescriptorDto File) : ICommand;
