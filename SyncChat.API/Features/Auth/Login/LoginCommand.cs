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

    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var userAuthProvider = await dbContext.UserAuthProviders
            .Include(uap => uap.User)
            .FirstOrDefaultAsync(x => x.Provider == AuthProvider.Local
                && EF.Functions.ILike(x.User.UserName, command.UserName), cancellationToken);

        if (userAuthProvider == null)
            return Result.Failure<LoginResponse>(UserErrors.InvalidUserNamePassword);

        User user = userAuthProvider.User;

        if(string.IsNullOrEmpty(user.PasswordHash) || 
            !passwordHasher.Verify(command.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidUserNamePassword);

        string accessToken = tokenProvider.GenerateAccessToken(user);
        string refreshToken = tokenProvider.GenerateRefreshToken();
        string refreshTokenHash = tokenProvider.HashRefreshToken(refreshToken);

        var now = DateTime.UtcNow;
        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.UserID
                && rt.DeviceIdentifier == command.DeviceIdentifier
                && rt.RevokedAt == null, cancellationToken);

        var newToken = RefreshTokenRules.Rotate(
            existingToken,
            user.UserID,
            refreshTokenHash,
            command.DeviceIdentifier,
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

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidUserNamePassword.Description)
            .WithErrorCode(UserErrors.InvalidUserNamePassword.Code);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidUserNamePassword.Description)
            .WithErrorCode(UserErrors.InvalidUserNamePassword.Code);

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidDeviceId.Description)
            .WithErrorCode(UserErrors.InvalidDeviceId.Code);
    }
}