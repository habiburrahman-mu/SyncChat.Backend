namespace SyncChat.API.Features.Auth.Refresh;

public sealed record RefreshResponse(string AccessToken, string RefreshToken);