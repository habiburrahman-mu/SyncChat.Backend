using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(string UserName, string Name, string Email, string Password)
    : ICommand<Result<Guid>>;
