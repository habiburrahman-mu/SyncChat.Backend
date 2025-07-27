namespace SyncChat.API.Shared.Socket.Contracts;

public interface IUserConnectionManager
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    IReadOnlyList<string> GetConnections(string userId);
    IReadOnlyList<string> GetAllConnectionsExcept(string userId);
}
