using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Token;

public sealed class TokenQueryHandler (ApplicationDbContext applicationDbContext, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
    : IQueryHandler<TokenQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(TokenQuery query, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        User? user = applicationDbContext.Users
            .FirstOrDefault(u => string.Equals(u.UserName, query.UserName, StringComparison.OrdinalIgnoreCase));

        if (user is null) return Result.Failure<string>(UserErrors.InvalidUserNamePassword);

        if(!passwordHasher.Verify(query.Password, user.PasswordHash)) 
            return Result.Failure<string>(UserErrors.InvalidUserNamePassword);

        return tokenProvider.GenerateToken(user);
    }
}