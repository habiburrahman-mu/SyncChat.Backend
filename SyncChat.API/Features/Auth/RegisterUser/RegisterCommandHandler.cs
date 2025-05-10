using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Entities;
using System.Text.Json;

namespace SyncChat.API.Features.Auth.RegisterUser;

public class RegisterCommandHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly ApplicationDbContext applicationDbContext;

    public RegisterCommandHandler(ApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }

    public async Task HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        User user = new()
        {
            UserID = 1,
            UUID = Guid.NewGuid(),
            UserName = command.userName,
            Email = command.email,
            Phone = null,
            PasswordHash = command.password,
            Profile = JsonDocument.Parse("{}"),
            Status = UserStatus.Offline,
            LastActive = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsVerified = false,
            IsBanned = false
        };

        applicationDbContext.Users.Add(user);

        await Task.CompletedTask;
    }
}
