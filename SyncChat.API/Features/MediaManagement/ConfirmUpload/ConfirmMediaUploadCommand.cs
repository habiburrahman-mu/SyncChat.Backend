using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;
using SyncChat.API.Shared.Storage.Contracts.Models;
using System.Windows.Input;

namespace SyncChat.API.Features.MediaManagement.ConfirmUpload;

public sealed record ConfirmMediaUploadCommand(Guid MediaId) : ICommand<ConfirmMediaUploadResult>;

public sealed class ConfirmMediaUploadCommandHandler : ICommandHandler<ConfirmMediaUploadCommand, ConfirmMediaUploadResult>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;

    public ConfirmMediaUploadCommandHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
    }

    public async Task<Result<ConfirmMediaUploadResult>> HandleAsync(ConfirmMediaUploadCommand command, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Media? media = await dbContext.Media.FirstOrDefaultAsync(x => x.Id == command.MediaId, cancellationToken);

        if (media is null)
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.NotFound(command.MediaId));
        }

        if (media.State != MediaState.Initiated) // Idempotency guard
        {
            return Result.Success(new ConfirmMediaUploadResult(MediaId: media.Id, MediaStateState: media.State));
        }

        MediaUploadSession? mediaUploadSession = await dbContext.MediaUploadSessions
            .Where(x => x.MediaId == media.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (mediaUploadSession is null)
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.UploadSessionNotFound(command.MediaId));
        }

        if (mediaUploadSession.ExpiresAt < now)
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.UploadSessionExpired(command.MediaId));
        }

        if (mediaUploadSession.UploadCount <= 0)
        {
            return Result.Failure<ConfirmMediaUploadResult>(
                MediaErrors.NoUploadAttemptDetected(command.MediaId));
        }

        BlobMetadata? metadata = await blobStorage.GetMetadataAsync(media.StorageKey, cancellationToken);

        if (metadata is null)
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.BlobMetadataNotFound(command.MediaId));
        }

        if (metadata.Size != media.SizeBytes)
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.InvalidBlobMeta(command.MediaId));
        }

        if (!string.Equals(
                metadata.ContentType,
                media.MimeType,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<ConfirmMediaUploadResult>(MediaErrors.InvalidBlobMeta(command.MediaId));
        }

        try
        {
            await dbContext.Database.BeginTransactionAsync();

            // Atomic state transition (concurrency-safe)
            int updated = await dbContext.Media
                .Where(x =>
                    x.Id == media.Id &&
                    x.State == MediaState.Initiated)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.State, MediaState.Uploaded)
                        .SetProperty(x => x.UpdatedAt, now),
                    cancellationToken);

            // Another confirm already won the race
            if (updated == 0)
            {
                return Result.Success(
                    new ConfirmMediaUploadResult(media.Id, MediaState.Uploaded));
            }

            mediaUploadSession.UsedAt = now;

            dbContext.MediaUploadSessions.Update(mediaUploadSession);

            await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.Database.CommitTransactionAsync(cancellationToken);

            return Result.Success(new ConfirmMediaUploadResult(MediaId: media.Id, MediaStateState: media.State));
        }
        catch
        {
            dbContext.Database.RollbackTransaction();

            throw;
        }
    }
}