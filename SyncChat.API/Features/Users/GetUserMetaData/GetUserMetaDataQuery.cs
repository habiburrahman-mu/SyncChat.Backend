using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.Users.GetUserMetaData;

public sealed record GetUserMetaDataQuery(
    long userId) : IQuery<GetUserMetaDataResponse>;

public sealed class GetUserMetaDataQueryHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    : IQueryHandler<GetUserMetaDataQuery, GetUserMetaDataResponse>
{
    public async Task<Result<GetUserMetaDataResponse>> HandleAsync(GetUserMetaDataQuery query, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Select(u => new
            {
                u.UserID,
                u.UserName,
                u.Name,
                u.AvatarKey
            })
            .FirstOrDefaultAsync(u => u.UserID == query.userId, cancellationToken);

        if (user is null)
            return Result.Failure<GetUserMetaDataResponse>(UserErrors.NotFound(query.userId));

        return Result.Success(new GetUserMetaDataResponse(
            user.UserID,
            user.UserName,
            user.Name,
            AvatarUrl: user.AvatarKey != null ? blobStorage.GetPublicObjectUrl(user.AvatarKey) : null));
    }
}
