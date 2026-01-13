using SyncChat.API.Infrastructure.AuthProviders.Google;

namespace SyncChat.API.Shared.AuthProviders.Contracts;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string token);
}
