using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Auth;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Events;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetRequest;

public sealed record PasswordResetRequestCommand(string EmailOrUserName) : ICommand;

public sealed class PasswordResetRequestCommandHandler : ICommandHandler<PasswordResetRequestCommand>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IPasswordResetTokenService passwordResetTokenService;
    private readonly IDomainEventPublisher domainEventPublisher;
    private readonly JWTSettings jwtSettings;

    public PasswordResetRequestCommandHandler(
        ApplicationDbContext dbContext,
        IPasswordResetTokenService passwordResetTokenService,
        IDomainEventPublisher domainEventPublisher,
        IOptions<JWTSettings> jwtOptions)
    {
        this.dbContext = dbContext;
        this.passwordResetTokenService = passwordResetTokenService;
        this.domainEventPublisher = domainEventPublisher;
        this.jwtSettings = jwtOptions.Value;
    }

    public async Task<Result> HandleAsync(PasswordResetRequestCommand command, CancellationToken cancellationToken = default)
    {
        string searchValue = command.EmailOrUserName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchValue))
            return Result.Success();

        var userAuthProvider = await dbContext.UserAuthProviders
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Provider == AuthProvider.Local
                && (EF.Functions.ILike(x.User.Email, searchValue) || EF.Functions.ILike(x.User.UserName, searchValue)), cancellationToken);

        if (userAuthProvider is null)
            return Result.Success();

        var user = userAuthProvider.User;
        var now = DateTimeOffset.UtcNow;

        await dbContext.PasswordResetTokens
            .Where(x => x.UserId == user.UserID && x.UsedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.UsedAt, now), cancellationToken);

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserID,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(jwtSettings.PasswordResetExpirationInMinutes)
        };

        await dbContext.PasswordResetTokens.AddAsync(resetToken, cancellationToken);

        await domainEventPublisher.PublishAsync(new SendPasswordResetEmailEvent
        {
            To = user.Email,
            FullName = user.Name,
            PasswordResetTokenId = resetToken.Id
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        domainEventPublisher.DispatchPendingEvents();

        return Result.Success();
    }
}

public sealed class PasswordResetRequestCommandValidator : AbstractValidator<PasswordResetRequestCommand>
{
    public PasswordResetRequestCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidUserName.Description)
            .WithErrorCode(UserErrors.InvalidUserName.Code);
    }
}
