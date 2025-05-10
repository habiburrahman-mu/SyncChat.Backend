using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence;

public class MockDb
{
    // A simple in-memory list of users
    private List<User> _users;

    public MockDb()
    {
        _users = new List<User>();
    }

    public List<User> Users 
    {
        get => _users;
    }
}
