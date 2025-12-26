using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken, string DeviceIdentifier) : ICommand;

public sealed class LogoutCommandHandler(
    ApplicationDbContext dbContext,
    ITokenProvider tokenProvider)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        string refreshTokenHash = tokenProvider.HashRefreshToken(command.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => 
                rt.TokenHash == refreshTokenHash &&
                rt.DeviceIdentifier == command.DeviceIdentifier &&
                rt.RevokedAt == null, cancellationToken);

        if (existingToken is null)
        {
            return Result.Success();
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        
        dbContext.RefreshTokens.Update(existingToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}