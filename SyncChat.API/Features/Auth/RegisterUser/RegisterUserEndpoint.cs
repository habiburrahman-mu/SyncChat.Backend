using Microsoft.AspNetCore.Http;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.RegisterUser;

public sealed class RegisterUserEndpoint : IAuthEndpoint
{
    public sealed record RegisterUserRequest(string UserName, string Name, string Email, string Password);

    public void Map(RouteGroupBuilder group)    
    {
        group.MapPost(AuthRoute.Register, async (RegisterUserRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        {
            RegisterUserCommand command = new(
                request.UserName,
                request.Name,
                request.Email,
                request.Password);

            Result<Guid> result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                id => Results.Created("", id),
                CustomResults.Problem);
        })
        .WithSummary("RegisterUser")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
