using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Users.GetUserByUserName;

public sealed record GetUserByUserNameQuery(string UserName) : IQuery<Result<GetUserByUserNameResponse?>>;

public sealed class GetUserByUserNameQueryHandler : IQueryHandler<GetUserByUserNameQuery, Result<GetUserByUserNameResponse?>>
{
    private readonly ApplicationDbContext _dbContext;
    public GetUserByUserNameQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetUserByUserNameResponse?>> HandleAsync(GetUserByUserNameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.UserName))
            return Result.Success<GetUserByUserNameResponse?>(null);

        var user = await _dbContext.Users
            .Where(u => EF.Functions.ILike(u.UserName.ToLower(), query.UserName.ToLower()))
            .Select(u => new GetUserByUserNameResponse(u.UserID, u.UUID, u.UserName, u.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}