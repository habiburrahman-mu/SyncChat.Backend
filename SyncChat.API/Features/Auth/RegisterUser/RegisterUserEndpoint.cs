using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.RegisterUser;

public class RegisterUserEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Register, async (RegisterUserRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        {
            RegisterUserCommand command = new(
                request.userName,
                request.email,
                request.password);

            await sender.SendAsync(command, cancellationToken);

            return Results.Created();
        });
    }
}
