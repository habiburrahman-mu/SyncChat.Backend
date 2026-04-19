using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.PasswordReset.PasswordResetComplete;

public sealed class PasswordResetCompleteEndpoint : IAuthEndpoint
{
    public sealed record ResetPasswordRequest(string Token, string NewPassword);

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.PasswordResetComplete, async (ResetPasswordRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        {
            var command = new PasswordResetCompleteCommand(request.Token, request.NewPassword);
            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                Results.NoContent,
                failure => CustomResults.Problem(failure));
        })
        .WithSummary("Reset password")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}