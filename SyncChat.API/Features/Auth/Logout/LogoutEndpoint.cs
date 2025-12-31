
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Logout;

public sealed record LogoutRequest(string DeviceIdentifier);

public class LogoutEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Logout,
            async ([FromBody] LogoutRequest request,
                ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                IRefreshTokenCookieManager refreshTokenCookieManager,
                CancellationToken cancellationToken) =>
        {
            if (!refreshTokenCookieManager.TryGet(httpContextAccessor.HttpContext!, out string refreshToken))
            {
                return Results.NoContent();
            }

            LogoutCommand command = new(RefreshToken: refreshToken, DeviceIdentifier: request.DeviceIdentifier);
            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                () =>
                {
                    refreshTokenCookieManager.Delete(httpContextAccessor.HttpContext!);
                    return Results.NoContent();
                },
                CustomResults.Problem);
        })
        .WithTags("Logout")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
