using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.LogoutAll;

public class LogoutAllEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.LogoutAll,
            async (ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                IRefreshTokenCookieManager refreshTokenCookieManager,
                CancellationToken cancellationToken) =>
        {
            if (!refreshTokenCookieManager.TryGet(httpContextAccessor.HttpContext!, out string refreshToken))
            {
                return Results.Unauthorized();
            }

            LogoutAllCommand command = new(RefreshToken: refreshToken);

            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                () =>
                {
                    refreshTokenCookieManager.Delete(httpContextAccessor.HttpContext!);

                    return Results.NoContent();
                },
                CustomResults.Problem);
        })
        .WithSummary("Logout All")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
