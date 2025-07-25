using SyncChat.API.Shared.Socket.Contracts;
using System.Collections.Concurrent;

namespace SyncChat.API.Infrastructure.Socket;

public class UserConnectionManager : IUserConnectionManager
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

    public void AddConnection(string userId, string connectionId)
    {
        var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
        lock (connections)
        {
            connections.Add(connectionId);
        }
    }

    public void RemoveConnection(string connectionId)
    {
        foreach (var (userId, connections) in _userConnections)
        {
            lock (connections)
            {
                if (connections.Remove(connectionId) && connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }
    }

    public IReadOnlyList<string> GetConnections(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }

        return Array.Empty<string>();
    }

    public IReadOnlyList<string> GetAllConnectionsExcept(string userId)
    {
        var result = new List<string>();

        foreach (var (uid, connections) in _userConnections)
        {
            if (uid == userId) continue;
            lock (connections)
            {
                result.AddRange(connections);
            }
        }

        return result;
    }
}
