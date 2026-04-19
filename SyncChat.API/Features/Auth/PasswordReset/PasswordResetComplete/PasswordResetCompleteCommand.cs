using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetComplete;

public sealed record PasswordResetCompleteCommand(string Token, string NewPassword) : ICommand;

public sealed class PasswordResetCompleteCommandHandler : ICommandHandler<PasswordResetCompleteCommand>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IPasswordHasher passwordHasher;
    private readonly IPasswordResetTokenService passwordResetTokenService;

    public PasswordResetCompleteCommandHandler(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenService passwordResetTokenService)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
        this.passwordResetTokenService = passwordResetTokenService;
    }

    public async Task<Result> HandleAsync(PasswordResetCompleteCommand command, CancellationToken cancellationToken = default)
    {
        if (!passwordResetTokenService.TryParseToken(command.Token, out var resetTokenId))
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        var resetToken = await dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == resetTokenId, cancellationToken);

        if (resetToken is null)
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        if (resetToken.UsedAt != null || resetToken.ExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure(UserErrors.InvalidPasswordResetToken);

        resetToken.UsedAt = DateTimeOffset.UtcNow;
        resetToken.User.PasswordHash = passwordHasher.Hash(command.NewPassword);
        resetToken.User.UpdatedAt = DateTimeOffset.UtcNow;

        var now = DateTime.UtcNow;

        await dbContext.RefreshTokens
            .Where(x => x.UserId == resetToken.UserId && x.RevokedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(r => r.RevokedAt, now), cancellationToken);

        await dbContext.PasswordResetTokens
            .Where(x => x.UserId == resetToken.UserId && x.UsedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(r => r.UsedAt, now), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class PasswordResetCompleteCommandValidator : AbstractValidator<PasswordResetCompleteCommand>
{
    public PasswordResetCompleteCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidPasswordResetToken.Description)
            .WithErrorCode(UserErrors.InvalidPasswordResetToken.Code);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must contain at least 8 characters.")
            .WithErrorCode("Users.Invalid.Password");
    }
}
