using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Events;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.MediaManagement.MediaUploaded;

public sealed class MediaUploadedEventHandler : IDomainEventHandler<MediaUploadedEvent>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;

    public MediaUploadedEventHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
    }

    public async Task HandleAsync(MediaUploadedEvent domainEvent, CancellationToken cancellationToken)
    {
        Media? media = await dbContext.Media.FirstOrDefaultAsync(x => x.Id == domainEvent.MediaId, cancellationToken);

        if (media is null || media.State != MediaState.Uploaded) return;

        media.State = MediaState.Active;
        await dbContext.SaveChangesAsync();

        // This block is done for future works like validate blob, generate thumbnails.

        // todo: notify
    }
}
