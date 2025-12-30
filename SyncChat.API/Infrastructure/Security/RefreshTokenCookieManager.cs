namespace SyncChat.API.Infrastructure.Security;

public interface IRefreshTokenCookieManager
{
    void Append(HttpContext httpContext, string refreshToken);

    /// <summary>
    /// Attempts to retrieve the refresh token from the HTTP request cookies.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the request from which to retrieve the refresh token. Cannot be null.</param>
    /// <param name="refreshToken">When this method returns, contains the refresh token value if found; otherwise, an empty string.</param>
    /// <returns>true if the refresh token was found in the request cookies; otherwise, false.</returns>
    bool TryGet(HttpContext httpContext, out string refreshToken);

    void Delete(HttpContext httpContext);
}

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
