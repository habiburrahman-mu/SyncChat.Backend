using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Sender.Contracts;

public interface IQueryHandler<TQuery>
    where TQuery : IQuery
{
    Task<Result> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}