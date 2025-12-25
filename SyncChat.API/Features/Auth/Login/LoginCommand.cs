using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Auth;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Login;

public sealed record LoginCommand(string UserName, string Password, string DeviceIdentifier) : ICommand<LoginResponse>;

public sealed class LoginCommandHandler(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IOptions<JWTSettings> jwtSettings)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly JWTSettings _jwtSettings = jwtSettings.Value;

    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand query, CancellationToken cancellationToken = default)
    {

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName.ToLower(), query.UserName), cancellationToken);

        if (user is null) return Result.Failure<LoginResponse>(UserErrors.InvalidUserNamePassword);

        if (!passwordHasher.Verify(query.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidUserNamePassword);

        if (string.IsNullOrWhiteSpace(query.DeviceIdentifier) || query.DeviceIdentifier.Length > 100)
            return Result.Failure<LoginResponse>(UserErrors.InvalidDeviceId);

        string accessToken = tokenProvider.GenerateAccessToken(user);
        string refreshToken = tokenProvider.GenerateRefreshToken();
        string refreshTokenHash = tokenProvider.HashRefreshToken(refreshToken);

        var now = DateTime.UtcNow;
        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.UserID
                && rt.DeviceIdentifier == query.DeviceIdentifier
                && rt.RevokedAt == null, cancellationToken);

        var newToken = RefreshTokenRules.Rotate(
            existingToken,
            user.UserID,
            refreshTokenHash,
            query.DeviceIdentifier,
            now,
            now.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes));

        if (existingToken != null)
            dbContext.RefreshTokens.Update(existingToken);

        dbContext.RefreshTokens.Add(newToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        LoginResponse response = new(AccessToken: accessToken, RefreshToken: refreshToken);

        return response;
    }
}