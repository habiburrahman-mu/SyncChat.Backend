using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetRequest;

public sealed class PasswordResetRequestEndpoint : IAuthEndpoint
{
    public sealed record PasswordResetRequest(string EmailOrUserName);

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.PasswordResetRequest, async (PasswordResetRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        {
            var command = new PasswordResetRequestCommand(request.EmailOrUserName);
            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                () => Results.Ok(),
                failure => CustomResults.Problem(failure));
        })
        .WithSummary("Request password reset")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}