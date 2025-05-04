namespace SyncChat.API.Shared.Sender.Contracts;

public interface ICommandSender
{
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
