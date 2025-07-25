namespace SyncChat.API.Shared.Socket;

public interface IUserConnectionManager
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string connectionId);
    IReadOnlyList<string> GetConnections(string userId);
    IReadOnlyList<string> GetAllConnectionsExcept(string userId);
}
