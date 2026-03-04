using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.MediaManagement.GetMediaAccessUrl;

public sealed class GetMediaAccessUrlEndpoint : IMediaEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(MediaRoute.GetAccessUrl,
            async (Guid mediaId, IQuerySender sender, CancellationToken cancellationToken) =>
            {
                GetMediaAccessUrlQuery query = new(mediaId);

                Result<GetMediaAccessUrlResponse> result = await sender.SendAsync(query, cancellationToken);

                return result.Match(
                    response => Results.Ok(response),
                    CustomResults.Problem);
            })
            .WithSummary("Get Media Access URL")
            .Produces<GetMediaAccessUrlResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
