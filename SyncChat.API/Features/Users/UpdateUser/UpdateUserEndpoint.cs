using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users.UpdateUser;

public sealed class UpdateUserEndpoint : IUserEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPut(UserRoute.Update + "/{userId:long}",
            async (long userId, [FromBody] UpdateUserRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                UpdateUserCommand command = new(
                    UserID: userId,
                    Name: request.Name,
                    Email: request.Email,
                    Phone: request.Phone);

                Result<UpdateUserResponse> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithSummary("Update User")
            .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
