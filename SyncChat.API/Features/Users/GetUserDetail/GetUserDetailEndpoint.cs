
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.GetUserDetail;

public sealed class GetUserDetailEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(UserRoute.GetUserDetail,
            async (long userId, IQuerySender sender, CancellationToken cancellationToken) =>
            {
                GetUserDetailQuery query = new();

                Result<GetUserDetailResponse> result = await sender.SendAsync(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .Produces<GetUserDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
