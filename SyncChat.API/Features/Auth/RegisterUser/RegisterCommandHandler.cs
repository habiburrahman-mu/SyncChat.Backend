using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Entities;
using System.Text.Json;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Security.Contracts;
using Microsoft.EntityFrameworkCore;

namespace SyncChat.API.Features.Auth.RegisterUser;

public sealed class RegisterCommandHandler(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(u => EF.Functions.ILike(u.UserName.ToLower(), command.UserName.ToLower()), cancellationToken))
            return Result.Failure<Guid>(UserErrors.UserNameNotUnique);

        if (await dbContext.Users.AnyAsync(u => EF.Functions.ILike(u.Email.ToLower(), command.Email.ToLower()), cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);

        User user = new()
        {
            UUID = Guid.NewGuid(),
            UserName = command.UserName,
            Name = command.Name,
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

        await dbContext.Users.AddAsync(user, cancellationToken);

        UserAuthProvider userAuthProvider = new UserAuthProvider
        {
            Email = command.Email,
            Provider = AuthProvider.Local,
            ProviderUserId = user.UUID.ToString(),
            LinkedAt = DateTimeOffset.UtcNow,
            User = user
        };

        await dbContext.UserAuthProviders.AddAsync(userAuthProvider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user.UUID;
    }
}
