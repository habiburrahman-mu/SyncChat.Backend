using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.MediaManagement.GetMediaState;

public sealed class GetMediaStateEndpoint : IMediaEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(MediaRoute.GetState,
            async (Guid mediaId, IQuerySender sender, CancellationToken cancellationToken) =>
            {
                GetMediaStateQuery query = new(mediaId);

                Result<GetMediaStateResponse> result = await sender.SendAsync(query, cancellationToken);

                return result.Match(
                    response => Results.Ok(response),
                    CustomResults.Problem);
            })
            .WithSummary("Get Media State")
            .Produces<GetMediaStateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
