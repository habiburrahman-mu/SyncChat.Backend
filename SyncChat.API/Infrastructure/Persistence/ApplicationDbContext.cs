using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence
{
    public class ApplicationDbContext
    {
        public ApplicationDbContext(MockDb mockDb)
        {
            Users = mockDb.Users;

        }

        public List<User> Users { get; }
    }
}
