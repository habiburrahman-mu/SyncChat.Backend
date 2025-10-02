
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.GetUserMetaData;

public sealed class GetUserMetaDataEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(UserRoute.GetMetaData, async (long userId, IQueryHandler<GetUserMetaDataQuery, GetUserMetaDataResponse> queryHandler, CancellationToken cancellationToken) =>
        {
            var query = new GetUserMetaDataQuery(userId);
            var result = await queryHandler.HandleAsync(query, cancellationToken);
            return result.Match(
                response => Results.Ok(response),
                CustomResults.Problem);
        })
        .WithSummary("Get User Meta Data")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .Produces<GetUserMetaDataResponse>(StatusCodes.Status200OK);
    }
}
