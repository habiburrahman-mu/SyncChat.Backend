namespace SyncChat.API.Shared.Sender.Contracts;

public interface IQuerySender
{
    Task SendAsync(IQuery query, CancellationToken cancellationToken = default);
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
