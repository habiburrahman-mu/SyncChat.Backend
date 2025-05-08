using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Infrastructure.Persistence
{
    public class ApplicationDbContext
    {
        public ApplicationDbContext()
        {
            Users = new();
        }

        public List<User> Users { get; }
    }
}
