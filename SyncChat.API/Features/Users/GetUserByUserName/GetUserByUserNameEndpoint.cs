
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.GetUserByUserName;

public class GetUserByUserNameEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(UserRoute.GetUserByUserName + "{userName}",
            async (string userName, CancellationToken cancellationToken) =>
            {

            });
    }
}
