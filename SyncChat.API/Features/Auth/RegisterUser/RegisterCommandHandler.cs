using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Entities;
using System.Text.Json;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Security.Contracts;

namespace SyncChat.API.Features.Auth.RegisterUser;

public sealed class RegisterCommandHandler(ApplicationDbContext applicationDbContext, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        if (applicationDbContext.Users.Any(u => string.Equals(u.UserName, command.UserName, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(UserErrors.UserNameNotUnique);

        if(applicationDbContext.Users.Any(u => string.Equals(u.Email, command.Email, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);

        User user = new()
        {
            UserID = 1,
            UUID = Guid.NewGuid(),
            UserName = command.UserName,
            Email = command.Email,
            Phone = null,
            PasswordHash = passwordHasher.Hash(command.Password),
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

        return user.UUID;
    }
}
