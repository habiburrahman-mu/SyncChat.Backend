namespace SyncChat.API.Features.Auth.Token;

public record TokenResponse(string Token, string RefreshToken, int ExpirationInMinutes);
