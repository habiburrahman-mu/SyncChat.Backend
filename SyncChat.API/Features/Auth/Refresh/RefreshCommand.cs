using FluentValidation;
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

namespace SyncChat.API.Features.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken, string DeviceIdentifier)
    : ICommand<RefreshResponse>;

public sealed class RefreshCommandHandler(
    IOptions<JWTSettings> options,
    ITokenProvider tokenProvider,
    ApplicationDbContext dbContext) : ICommandHandler<RefreshCommand, RefreshResponse>
{
    private readonly JWTSettings _jwtSettings = options.Value;

    public async Task<Result<RefreshResponse>> HandleAsync(RefreshCommand query, CancellationToken cancellationToken = default)
    {
        string refreshTokenHash = tokenProvider.HashRefreshToken(query.RefreshToken);

        RefreshToken? existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == refreshTokenHash &&
                rt.DeviceIdentifier == query.DeviceIdentifier &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (existingToken is null)
        {
            return Result.Failure<RefreshResponse>(UserErrors.InvalidRefreshToken);
        }

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserID == existingToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<RefreshResponse>(UserErrors.NotFound(existingToken.UserId));
        }

        string newRefreshToken = tokenProvider.GenerateRefreshToken();
        string newRefreshTokenHash = tokenProvider.HashRefreshToken(newRefreshToken);
        string newAccessToken = tokenProvider.GenerateAccessToken(user);

        RefreshToken rotatedToken = RefreshTokenRules.Rotate(
            existingToken,
            existingToken.UserId,
            newRefreshTokenHash,
            query.DeviceIdentifier,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes));

        dbContext.RefreshTokens.Update(existingToken);
        dbContext.RefreshTokens.Add(rotatedToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshResponse(AccessToken: newAccessToken, RefreshToken: newRefreshToken));
    }
}

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(rc => rc.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");

        RuleFor(rc => rc.DeviceIdentifier)
            .NotEmpty()
            .WithMessage("Device identifier is required.");
    }
}
