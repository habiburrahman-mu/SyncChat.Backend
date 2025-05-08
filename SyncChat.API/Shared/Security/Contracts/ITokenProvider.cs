using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Shared.Security.Contracts;

public interface ITokenProvider
{
    string GenerateToken(User user);
}
