using SyncChat.API.Shared.Security.Contracts;

namespace SyncChat.API.Infrastructure.Security;

public sealed class RefreshTokenCookieManager(
    ICookieOptionsProvider cookieOptionsProvider) : IRefreshTokenCookieManager
{
    private const string CookieName = "refreshToken";

    public void Append(HttpContext httpContext, string refreshToken)
    {
        httpContext.Response.Cookies.Append(
            CookieName,
            refreshToken,
            cookieOptionsProvider.CreateRefreshTokenOptions());
    }

    public bool TryGet(HttpContext httpContext, out string refreshToken)
    {
        string? refreshTokenValue = null;

        if (httpContext.Request.Cookies.TryGetValue(CookieName, out refreshTokenValue))
        {
            if (refreshTokenValue is null)
            {
                refreshToken = string.Empty;
                return false;
            }

            refreshToken = refreshTokenValue;

            return true;
        }

        refreshToken = string.Empty;

        return false;
    }

    public void Delete(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieName);
    }
}
