
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Logout;

public sealed record LogoutRequest(string DeviceIdentifier);

public class LogoutEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Logout,
            async ([FromBody] LogoutRequest request,
                ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                CancellationToken cancellationToken) =>
        {
            if (!httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return CustomResults.Problem(Result.Failure(AuthErrors.Unauthorized()));
            }

            LogoutCommand command = new(RefreshToken: refreshToken, DeviceIdentifier: request.DeviceIdentifier);
            Result result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                () =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;
                    httpContext.Response.Cookies.Delete("refreshToken");
                    return Results.NoContent();
                },
                CustomResults.Problem);
        })
        .WithTags("Logout")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
