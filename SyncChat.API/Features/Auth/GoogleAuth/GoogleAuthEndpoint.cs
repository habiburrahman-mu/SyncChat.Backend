
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.GoogleAuth;

public sealed record GoogleAuthRequest(string idToken, string deviceIdentifier);

public sealed class GoogleAuthEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.GoogleAuth,
            async ([FromBody] GoogleAuthRequest request,
                ICommandSender commandSender,
                CancellationToken cancellationToken) =>
            {

            });
    }
}
