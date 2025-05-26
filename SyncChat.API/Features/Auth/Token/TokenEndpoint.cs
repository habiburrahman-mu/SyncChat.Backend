using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Token;

public class TokenEndpoint : IAuthEndpoint
{
    public sealed record TokenRequest(string UserName, string Password) : ICommand<string>;

    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(AuthRoute.Token, async ([FromBody]TokenRequest request, IQuerySender sender, CancellationToken cancellationToken) =>
        {
            TokenQuery query = new(request.UserName, request.Password);

            Result<string> result = await sender.SendAsync(query, cancellationToken);

            return result.Match(
                token => Results.Ok(token),
                CustomResults.Problem);
        })
        .WithSummary("Token")
        .Produces<string>(StatusCodes.Status200OK);
    }
}
