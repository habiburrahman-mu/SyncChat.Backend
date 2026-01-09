
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
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
                CancellationToken cancellationToken) =>
            {
                var command = new GoogleAuthCommand(
                    IdToken: request.IdToken,
                    DeviceIdentifier: request.DeviceIdentifier);

                var result = await sender.SendAsync(command, cancellationToken);
                
                return result.Match(
                    success => Results.Ok(success),
                    CustomResults.Problem);
            });
    }
}
