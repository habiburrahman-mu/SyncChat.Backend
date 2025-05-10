using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.RegisterUser;

public record RegisterUserCommand(
    string userName,
    string email,
    string password) : ICommand;
