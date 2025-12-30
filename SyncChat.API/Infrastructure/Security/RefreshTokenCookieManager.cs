namespace SyncChat.API.Infrastructure.Security;

public interface IRefreshTokenCookieManager
{
    void Append(HttpContext httpContext, string refreshToken);
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

    public void Delete(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieName);
    }
}
