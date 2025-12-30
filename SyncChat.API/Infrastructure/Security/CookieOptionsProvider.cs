using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;

namespace SyncChat.API.Infrastructure.Security;

public interface ICookieOptionsProvider
{
    CookieOptions CreateRefreshTokenOptions();
}

public sealed class CookieOptionsProvider(
    IWebHostEnvironment environment,
    IOptions<JWTSettings> options) : ICookieOptionsProvider
{
    private readonly JWTSettings _settings = options.Value;

    public CookieOptions CreateRefreshTokenOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = environment.IsDevelopment()
                        ? SameSiteMode.Lax
                        : SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(_settings.RefreshTokenExpirationInMinutes)
        };
    }
}
