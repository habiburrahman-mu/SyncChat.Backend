namespace SyncChat.API.Features.Auth.RegisterUser;

public record RegisterUserRequest(
    string userName,
    string email,
    string password);