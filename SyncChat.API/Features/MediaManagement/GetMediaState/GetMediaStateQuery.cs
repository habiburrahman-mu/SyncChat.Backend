using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.MediaManagement.GetMediaState;

public sealed record GetMediaStateQuery(Guid MediaId) : IQuery<GetMediaStateResponse>;

public sealed class GetMediaStateQueryHandler : IQueryHandler<GetMediaStateQuery, GetMediaStateResponse>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IIdentityService identityService;

    public GetMediaStateQueryHandler(
        ApplicationDbContext dbContext,
        IIdentityService identityService)
    {
        this.dbContext = dbContext;
        this.identityService = identityService;
    }

    public async Task<Result<GetMediaStateResponse>> HandleAsync(
        GetMediaStateQuery query,
        CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        Media? media = await dbContext.Media
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.MediaId, cancellationToken);

        if (media is null)
            return Result.Failure<GetMediaStateResponse>(MediaErrors.NotFound(query.MediaId));

        bool isOwner = await dbContext.Users
            .AnyAsync(u => u.UserID == currentUserId && u.UUID == media.UserId, cancellationToken);

        if (!isOwner)
            return Result.Failure<GetMediaStateResponse>(MediaErrors.Forbidden);

        return new GetMediaStateResponse(
            MediaId: media.Id,
            MediaState: (int)media.State);
    }
}

public sealed class GetMediaStateQueryValidator : AbstractValidator<GetMediaStateQuery>
{
    public GetMediaStateQueryValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Media ID is required.");
    }
}
