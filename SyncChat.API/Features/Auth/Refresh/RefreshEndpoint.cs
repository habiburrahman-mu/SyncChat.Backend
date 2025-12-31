using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Refresh;

public sealed record RefreshRequest(string DeviceIdentifier);

public sealed class RefreshEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Refresh,
            async([FromBody] RefreshRequest request,
                ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                IRefreshTokenCookieManager refreshTokenCookieManager,
                CancellationToken cancellationToken) =>
        {
            if (!refreshTokenCookieManager.TryGet(httpContextAccessor.HttpContext!, out string refreshToken))
            {
                return Results.Unauthorized();
            }

            RefreshCommand query = new(RefreshToken: refreshToken, DeviceIdentifier: request.DeviceIdentifier);

            Result<RefreshResponse> result = await sender.SendAsync(query, cancellationToken);

            return result.Match(
                response =>
                {
                    refreshTokenCookieManager.Append(httpContextAccessor.HttpContext!, response.RefreshToken);

                    return Results.Ok(response.AccessToken);
                },
                CustomResults.Problem);
        })
        .WithSummary("Refresh Token")
        .Produces<string>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
