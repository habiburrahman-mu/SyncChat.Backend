namespace SyncChat.API.Shared.Sender.Contracts;

public interface ICommand
{
}

public interface ICommand<TResponse> : ICommand
{
}