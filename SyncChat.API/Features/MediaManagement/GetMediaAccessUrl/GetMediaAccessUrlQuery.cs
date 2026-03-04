using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.MediaManagement.GetMediaAccessUrl;

public sealed record GetMediaAccessUrlQuery(Guid MediaId) : IQuery<GetMediaAccessUrlResponse>;

public sealed class GetMediaAccessUrlQueryHandler : IQueryHandler<GetMediaAccessUrlQuery, GetMediaAccessUrlResponse>
{
    private static readonly TimeSpan PresignedUrlTtl = TimeSpan.FromMinutes(15);

    private readonly ApplicationDbContext dbContext;
    private readonly IBlobStorage blobStorage;
    private readonly IIdentityService identityService;

    public GetMediaAccessUrlQueryHandler(
        ApplicationDbContext dbContext,
        IBlobStorage blobStorage,
        IIdentityService identityService)
    {
        this.dbContext = dbContext;
        this.blobStorage = blobStorage;
        this.identityService = identityService;
    }

    public async Task<Result<GetMediaAccessUrlResponse>> HandleAsync(
        GetMediaAccessUrlQuery query,
        CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        Media? media = await dbContext.Media
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.MediaId, cancellationToken);

        if (media is null)
            return Result.Failure<GetMediaAccessUrlResponse>(MediaErrors.NotFound(query.MediaId));

        if (media.State is not (MediaState.Active or MediaState.Attached))
            return Result.Failure<GetMediaAccessUrlResponse>(MediaErrors.MediaIsNotActive);

        bool hasAccess = await CheckAccessAsync(media, currentUserId, cancellationToken);

        if (!hasAccess)
            return Result.Failure<GetMediaAccessUrlResponse>(MediaErrors.Forbidden);

        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(PresignedUrlTtl);

        string url = await blobStorage.GeneratePresignedDownloadUrlAsync(
            media.StorageKey,
            PresignedUrlTtl,
            cancellationToken);

        return new GetMediaAccessUrlResponse(
            MediaId: media.Id,
            Url: url,
            ExpiresAt: expiresAt);
    }

    private async Task<bool> CheckAccessAsync(Media media, long currentUserId, CancellationToken cancellationToken)
    {
        if (media.OwnerType == MediaOwnerType.Conversation &&
            long.TryParse(media.OwnerId, out long conversationId))
        {
            return await dbContext.ConversationMembers
                .AnyAsync(cm =>
                    cm.ConversationId == conversationId &&
                    cm.UserId == currentUserId &&
                    cm.IsActive,
                    cancellationToken);
        }

        return await dbContext.Users
            .AnyAsync(u => u.UserID == currentUserId && u.UUID == media.UserId, cancellationToken);
    }
}

public sealed class GetMediaAccessUrlQueryValidator : AbstractValidator<GetMediaAccessUrlQuery>
{
    public GetMediaAccessUrlQueryValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Media ID is required.");
    }
}
