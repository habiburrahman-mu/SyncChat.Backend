using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Users.GetUserDetail;

public sealed record GetUserDetailQuery(): IQuery<GetUserDetailResponse>;

public sealed class GetUserDetailQueryHandler : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    public Task<Result<GetUserDetailResponse>> HandleAsync(GetUserDetailQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
