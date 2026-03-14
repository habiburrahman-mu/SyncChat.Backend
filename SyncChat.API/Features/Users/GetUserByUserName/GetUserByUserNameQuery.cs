using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.Users.GetUserByUserName;

public sealed record GetUserByUserNameQuery(string UserName) : IQuery<GetUserByUserNameResponse>;

public sealed class GetUserByUserNameQueryHandler : IQueryHandler<GetUserByUserNameQuery, GetUserByUserNameResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBlobStorage _blobStorage;

    public GetUserByUserNameQueryHandler(ApplicationDbContext dbContext, IBlobStorage blobStorage)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
    }

    public async Task<Result<GetUserByUserNameResponse>> HandleAsync(GetUserByUserNameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.UserName))
            return Result.Failure<GetUserByUserNameResponse>(UserErrors.InvalidUserName);

        var user = await _dbContext.Users
            .Where(u => EF.Functions.ILike(u.UserName.ToLower(), query.UserName.ToLower()))
            .Select(u => new { u.UserID, u.UUID, u.UserName, u.Name, u.AvatarKey })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<GetUserByUserNameResponse>(UserErrors.UserNameNotFound(query.UserName));

        return new GetUserByUserNameResponse(
            user.UserID,
            user.UUID,
            user.UserName,
            user.Name,
            AvatarUrl: user.AvatarKey != null ? _blobStorage.GetPublicObjectUrl(user.AvatarKey) : null);
    }
}