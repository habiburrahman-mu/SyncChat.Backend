namespace SyncChat.API.Shared.Sender.Contracts;

public interface IQueryHandler<TQuery>
    where TQuery : IQuery
{
    Task HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}