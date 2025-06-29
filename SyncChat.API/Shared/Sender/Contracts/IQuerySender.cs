using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Sender.Contracts;

public interface IQuerySender
{
    Task<Result> SendAsync(IQuery query, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
