using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SyncChat.API.Features.Auth.Login;
using SyncChat.API.Infrastructure.AuthProviders.Google;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Auth;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.GoogleAuth;

public sealed record GoogleAuthCommand(string IdToken, string DeviceIdentifier) : ICommand<GoogleAuthResponse>;

public sealed class GoogleAuthCommandHandler : ICommandHandler<GoogleAuthCommand, GoogleAuthResponse>
{
    private readonly IGoogleTokenValidator googleTokenValidator;
    private readonly ApplicationDbContext dbContext;
    private readonly ITokenProvider tokenProvider;
    private readonly JWTSettings jwtSettings;

    public GoogleAuthCommandHandler(IGoogleTokenValidator googleTokenValidator, ApplicationDbContext applicationDbContext, ITokenProvider tokenProvider, IOptions<JWTSettings> jwtOptions)
    {
        this.googleTokenValidator = googleTokenValidator;
        this.dbContext = applicationDbContext;
        this.tokenProvider = tokenProvider;
        this.jwtSettings = jwtOptions.Value;
    }

    public async Task<Result<GoogleAuthResponse>> HandleAsync(GoogleAuthCommand command, CancellationToken cancellationToken)
    {
        var googleUserInfo = await googleTokenValidator.ValidateAsync(command.IdToken);

        if (googleUserInfo is null)
        {
            return Result.Failure<GoogleAuthResponse>(AuthErrors.InvalidCredentials);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            User user = await FindOrCreateUserAsync(googleUserInfo, command.DeviceIdentifier, cancellationToken);

            if (user.IsBanned)
            {
                return Result.Failure<GoogleAuthResponse>(AuthErrors.Unauthorized());
            }

            var providerExists = await dbContext.UserAuthProviders.AnyAsync(
                    p => p.UserID == user.UserID &&
                         p.Provider == AuthProvider.Google,
                    cancellationToken);

            if (!providerExists)
            {
                await dbContext.UserAuthProviders.AddAsync(new UserAuthProvider
                {
                    User = user,
                    Provider = AuthProvider.Google,
                    ProviderUserId = googleUserInfo.Subject,
                    Email = googleUserInfo.Email,
                    LinkedAt = DateTimeOffset.UtcNow
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = tokenProvider.GenerateAccessToken(user);
            var refreshToken = tokenProvider.GenerateRefreshToken();
            var refreshTokenHash = tokenProvider.HashRefreshToken(refreshToken);

            var existingToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == user.UserID
                    && rt.DeviceIdentifier == command.DeviceIdentifier
                    && rt.RevokedAt == null, cancellationToken);

            var now = DateTime.UtcNow;

            dbContext.RefreshTokens.Add(
                RefreshTokenRules.Rotate(
                    existingToken,
                    user.UserID,
                    refreshTokenHash,
                    command.DeviceIdentifier,
                    now,
                    now.AddMinutes(jwtSettings.RefreshTokenExpirationInMinutes)));

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new GoogleAuthResponse(AccessToken: accessToken, RefreshToken: refreshToken);
        }
        catch (Exception)
        {
            throw; // rollback via dispose
        }
    }

    private async Task<User> FindOrCreateUserAsync(GoogleUserInfo googleUserInfo, string deviceIdentifier, CancellationToken cancellationToken)
    {
        string googleEmail = googleUserInfo.Email.ToLower();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == googleEmail, cancellationToken);

        if (user is null)
        {
            string generatedUserName = await GenerateUniqueUserNameAsync(googleUserInfo.Email, cancellationToken);

            user = new User
            {
                UUID = Guid.NewGuid(),
                UserName = generatedUserName,
                Name = googleUserInfo.Name,
                Email = googleEmail,
                Phone = null,
                PasswordHash = null,
                Profile = System.Text.Json.JsonDocument.Parse("{}"),
                Status = UserStatus.Offline,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                IsVerified = googleUserInfo.EmailVerified,
                IsBanned = false
            };

            await dbContext.Users.AddAsync(user, cancellationToken);
        }

        return user;
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email, CancellationToken cancellationToken)
    {
        var baseName = email.Split('@')[0].ToLowerInvariant();

        for (int attempt = 0; attempt < 10; attempt++)
        {
            var candidate = attempt == 0
                ? baseName
                : $"{baseName}{Random.Shared.Next(100, 9999)}";

            var exists = await dbContext.Users
                .AnyAsync(u => EF.Functions.ILike(u.UserName, candidate), cancellationToken);

            if (!exists)
                return candidate;
        }

        // Fallback that is guaranteed unique
        return $"{baseName}{Guid.NewGuid():N[..6]}";
    }
}

public sealed class GoogleAuthCommandValidator : AbstractValidator<GoogleAuthCommand>
{
    public GoogleAuthCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("ID token must not be empty.");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty()
            .WithMessage("Device identifier must not be empty.");
    }
}