using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using System.Linq;

namespace SyncChat.API.Features.Auth.Token;

public sealed class TokenQueryHandler(ApplicationDbContext dbContext, IPasswordHasher passwordHasher, ITokenProvider tokenProvider, IOptions<JWTSettings> jwtSettings)
    : IQueryHandler<TokenQuery, TokenResponse>
{
    private readonly JWTSettings _jwtSettings = jwtSettings.Value;

    public async Task<Result<TokenResponse>> HandleAsync(TokenQuery query, CancellationToken cancellationToken = default)
    {

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName.ToLower(), query.UserName), cancellationToken);

        if (user is null) return Result.Failure<TokenResponse>(UserErrors.InvalidUserNamePassword);

        if (!passwordHasher.Verify(query.Password, user.PasswordHash))
            return Result.Failure<TokenResponse>(UserErrors.InvalidUserNamePassword);

        string token = tokenProvider.GenerateAccessToken(user);

        TokenResponse response = new(
            Token: token,
            RefreshToken: "demo-refresh-token");

        return response;
    }
}