using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Shared.Security.Contracts;

public interface ITokenProvider
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
