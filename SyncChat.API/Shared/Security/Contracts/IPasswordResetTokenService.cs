namespace SyncChat.API.Shared.Security.Contracts;

public interface IPasswordResetTokenService
{
    string CreateToken(Guid resetTokenId);
    bool TryParseToken(string token, out Guid resetTokenId);
}
