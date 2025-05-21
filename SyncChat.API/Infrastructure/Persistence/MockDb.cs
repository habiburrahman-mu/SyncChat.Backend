using SyncChat.API.Shared.Entities;
using System.Text.Json;

namespace SyncChat.API.Infrastructure.Persistence;

public class MockDb
{
    // A simple in-memory list of users
    private List<User> _users;

    public MockDb()
    {
        _users = new List<User>()
        {
            new()
            {
                UserID = 1,
                UUID = Guid.Parse("e7424d80-d60a-4746-b521-3dc1f91525ad"),
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Phone = null,
                PasswordHash = "5442A43E152BE7FBE26E98E0623CE4523AFE00AF051A5FC4260B783A953F0D2F-3279916E982569554B6C720497387BDB", // password123
                Profile = JsonDocument.Parse("{}"),
                Status = UserStatus.Offline,
                LastActive = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                IsVerified = false,
                IsBanned = false
            }
        };
    }

    public List<User> Users
    {
        get => _users;
    }
}
