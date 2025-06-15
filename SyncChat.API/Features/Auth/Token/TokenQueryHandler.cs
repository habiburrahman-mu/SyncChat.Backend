using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using System.Linq;

namespace SyncChat.API.Features.Auth.Token;

public sealed class TokenQueryHandler(ApplicationDbContext dbContext, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
    : IQueryHandler<TokenQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(TokenQuery query, CancellationToken cancellationToken = default)
    {

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName.ToLower(), query.UserName), cancellationToken);

        if (user is null) return Result.Failure<string>(UserErrors.InvalidUserNamePassword);

        if (!passwordHasher.Verify(query.Password, user.PasswordHash))
            return Result.Failure<string>(UserErrors.InvalidUserNamePassword);

        return tokenProvider.GenerateToken(user);
    }
}