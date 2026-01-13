using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.GoogleAuth;

public sealed record GoogleAuthRequest(string IdToken, string DeviceIdentifier);

public sealed class GoogleAuthEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.GoogleAuth,
            async ([FromBody] GoogleAuthRequest request,
                ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                IRefreshTokenCookieManager refreshTokenCookieManager,
                CancellationToken cancellationToken) =>
            {
                var command = new GoogleAuthCommand(
                    IdToken: request.IdToken,
                    DeviceIdentifier: request.DeviceIdentifier);

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(
                    (token) =>
                        {
                            refreshTokenCookieManager.Append(httpContextAccessor.HttpContext!, token.RefreshToken);

                            return Results.Ok(token.AccessToken);
                        },
                    CustomResults.Problem);
            });
    }
}
