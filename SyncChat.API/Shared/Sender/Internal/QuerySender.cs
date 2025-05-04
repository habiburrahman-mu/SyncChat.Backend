using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Shared.Sender.Internal;

public class QuerySender(IServiceProvider serviceProvider) : IQuerySender
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public Task SendAsync(IQuery query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<>).MakeGenericType(query.GetType());
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, cancellationToken);
    }
}
