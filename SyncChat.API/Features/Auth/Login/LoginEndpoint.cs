using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Login;

public class LoginEndpoint : IAuthEndpoint
{
    public sealed record LoginRequest(string UserName, string Password, string DeviceIdentifier) : ICommand<string>;

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Login, async ([FromBody] LoginRequest request, IQuerySender sender, CancellationToken cancellationToken) =>
        {
            LoginQuery query = new(request.UserName, request.Password, request.DeviceIdentifier);

            Result<LoginResponse> result = await sender.SendAsync(query, cancellationToken);

            return result.Match(
                token => Results.Ok(token),
                CustomResults.Problem);
        })
        .WithSummary("Login")
        .Produces<string>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
