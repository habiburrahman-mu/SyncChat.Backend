using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Refresh;

public sealed class RefreshEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(AuthRoute.RefreshToken, async (IQuerySender sender, CancellationToken cancellationToken) =>
        {
            RefreshQuery query = new();

            Result<RefreshResponse> result = await sender.SendAsync(query, cancellationToken);

            return result.Match(
                response => Results.Ok(response),
                CustomResults.Problem);
        })
        .WithTags("Refresh Token")
        .Produces<RefreshResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
