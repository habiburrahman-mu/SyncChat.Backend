
using Microsoft.AspNetCore.JsonPatch;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.UpdateUser;

public sealed class UpdateUserEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPatch(UserRoute.Update + "/{userId:long}",
            async (long userId, JsonPatchDocument<UpdateUserRequest> patchDocument, ICommandSender sender, CancellationToken cancellationToken) =>
            {

            });
    }
}
