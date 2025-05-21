namespace SyncChat.API.Shared.Security.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);   
    bool Verify(string password, string hashedPassword);
}
