using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Users.GetUserDetail;

public sealed record GetUserDetailQuery() : IQuery<GetUserDetailResponse>;

public sealed class GetUserDetailQueryHandler(IIdentityService identityService, ApplicationDbContext dbContext)
    : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    public async Task<Result<GetUserDetailResponse>> HandleAsync(GetUserDetailQuery query, CancellationToken cancellationToken = default)
    {
        long userId = identityService.GetUserID();

        var user = await dbContext.Users
            .Where(u => u.UserID == userId)
            .Select(u => new GetUserDetailResponse (
                u.UserID,
                u.UUID,
                u.UserName,
                u.Name,
                u.Email,
                u.Phone,
                u.Profile,
                u.Status,
                u.LastActive,
                u.CreatedAt,
                u.IsVerified,
                u.IsBanned
            ))
            .FirstOrDefaultAsync();

        if (user is null)
            return Result.Failure<GetUserDetailResponse>(UserErrors.NotFound(userId));

        return user;
    }
}
