
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.RefreshToken;

public sealed record RefreshTokenQuery() : IQuery<RefreshTokenResponse>;

public sealed class RefreshTokenQueryHandler() : IQueryHandler<RefreshTokenQuery, RefreshTokenResponse>
{
    public Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
