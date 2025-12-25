using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Refresh;

public sealed record RefreshQuery() : IQuery<RefreshResponse>;

public sealed class RefreshTokenQueryHandler() : IQueryHandler<RefreshQuery, RefreshResponse>
{
    public Task<Result<RefreshResponse>> HandleAsync(RefreshQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
