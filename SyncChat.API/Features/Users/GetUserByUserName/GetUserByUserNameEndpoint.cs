
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.GetUserByUserName;

public class GetUserByUserNameEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(UserRoute.GetUserByUserName + "{userName}",
            async (string userName, IQuerySender sender, CancellationToken cancellationToken) =>
            {
                GetUserByUserNameQuery query = new(userName);

                Result<GetUserByUserNameResponse?> result = await sender.SendAsync(query, cancellationToken);

                return result.Match(
                    user => user is not null ? Results.Ok(user) : Results.NotFound(),
                    CustomResults.Problem);
            });
    }
}
