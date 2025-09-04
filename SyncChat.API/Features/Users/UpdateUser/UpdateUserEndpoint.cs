
using Microsoft.AspNetCore.JsonPatch;
using SyncChat.API.Shared.ResultHandling;
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
                UpdateUserCommand command = new(UserID: userId, PatchDocument: patchDocument);

                Result<UpdateUserResponse> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
