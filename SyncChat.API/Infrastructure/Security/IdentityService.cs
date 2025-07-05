using System.Security.Claims;

namespace SyncChat.API.Infrastructure.Security;

public interface IIdentityService
{
    long GetUserID();
}

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("No HttpContext.");

    public long GetUserID()
    {
        var name = User.Identity?.Name ?? string.Empty;
        return long.TryParse(name, out var id) ? id : 0;
    }
}