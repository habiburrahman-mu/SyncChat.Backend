using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.LogoutAll;

public sealed record LogoutAllCommand(string RefreshToken) : ICommand;

public sealed class LogoutAllCommandHandler(
    ApplicationDbContext dbContext,
    ITokenProvider tokenProvider)
    : ICommandHandler<LogoutAllCommand>
{
    public async Task<Result> HandleAsync(LogoutAllCommand command, CancellationToken cancellationToken = default)
    {
        string refreshTokenHash = tokenProvider.HashRefreshToken(command.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => 
                rt.TokenHash == refreshTokenHash &&
                rt.RevokedAt == null, cancellationToken);

        if (existingToken is null)
        {
            return Result.Success();
        }

        var userId = existingToken.UserId;

        var now = DateTime.UtcNow;

        await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(rt => rt.SetProperty(r => r.RevokedAt, now), cancellationToken);

        return Result.Success();
    }
}