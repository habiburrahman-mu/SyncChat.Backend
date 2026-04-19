using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetVerify;

public sealed record PasswordResetVerifyCommand(string Token) : ICommand;

public sealed class PasswordResetVerifyCommandHandler : ICommandHandler<PasswordResetVerifyCommand>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IPasswordResetTokenService passwordResetTokenService;

    public PasswordResetVerifyCommandHandler(
        ApplicationDbContext dbContext,
        IPasswordResetTokenService passwordResetTokenService)
    {
        this.dbContext = dbContext;
        this.passwordResetTokenService = passwordResetTokenService;
    }

    public async Task<Result> HandleAsync(PasswordResetVerifyCommand command, CancellationToken cancellationToken = default)
    {
        if (!passwordResetTokenService.TryParseToken(command.Token, out var resetTokenId))
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        var resetToken = await dbContext.PasswordResetTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == resetTokenId, cancellationToken);

        if (resetToken is null)
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        if (resetToken.UsedAt != null)
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        if (resetToken.ExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure(UserErrors.PasswordResetTokenExpired);

        return Result.Success();
    }
}
