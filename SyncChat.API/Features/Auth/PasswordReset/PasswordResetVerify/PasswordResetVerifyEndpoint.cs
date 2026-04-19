using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetVerify;

public sealed class PasswordResetVerifyEndpoint : IAuthEndpoint
{
    public sealed record PasswordResetVerifyRequest(string Token);

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.PasswordResetVerify, async (PasswordResetVerifyRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        {
            var command = new PasswordResetVerifyCommand(request.Token);
            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                () => Results.Ok(),
                failure => CustomResults.Problem(failure));
        })
        .WithSummary("Verify password reset token")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}