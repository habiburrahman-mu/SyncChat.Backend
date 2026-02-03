using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.Media.InitiateUpload;

public sealed record InitiateUploadCommand(
    MediaOwnerDto Owner,
    MediaFileDescriptorDto File) : ICommand<InitiateUploadResponse>;

public sealed class InitiateUploadCommandHandler : ICommandHandler<InitiateUploadCommand, InitiateUploadResponse>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;

    public InitiateUploadCommandHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
    }

    public Task<Result<InitiateUploadResponse>> HandleAsync(InitiateUploadCommand request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
