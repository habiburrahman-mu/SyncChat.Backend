namespace SyncChat.API.Features.Auth.GoogleAuth;

public sealed record GoogleAuthResponse(string AccessToken, string RefreshToken);