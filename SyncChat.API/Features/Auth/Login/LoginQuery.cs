using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Auth.Login;

public sealed record LoginQuery(string UserName, string Password, string DeviceIdentifier) : IQuery<LoginResponse>;

public sealed class LoginQueryHandler(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IOptions<JWTSettings> jwtSettings,
    IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<LoginQuery, LoginResponse>
{
    private readonly JWTSettings _jwtSettings = jwtSettings.Value;
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    public async Task<Result<LoginResponse>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
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

        await SaveRefreshTokenAsync(
            user.UserID,
            refreshToken,
            query.DeviceIdentifier, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes)
        };

        _httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        LoginResponse response = new(Token: accessToken);

        return response;
    }

    private async Task SaveRefreshTokenAsync(
        long userId,
        string refreshTokenHash,
        string deviceIdentifier, CancellationToken cancellationToken)
    {
        var existingToken = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.DeviceIdentifier == deviceIdentifier && rt.RevokedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken != null)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            dbContext.RefreshTokens.Update(existingToken);
        }

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = refreshTokenHash,
            DeviceIdentifier = deviceIdentifier,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}