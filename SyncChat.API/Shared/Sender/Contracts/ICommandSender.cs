using SyncChat.API.Shared.ResultHandling;

namespace SyncChat.API.Shared.Sender.Contracts;

public interface ICommandSender
{
    Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
