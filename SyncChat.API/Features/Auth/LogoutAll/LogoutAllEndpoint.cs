
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.LogoutAll;

public class LogoutAllEndpoint : IAuthEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.LogoutAll,
            async (ICommandSender sender,
                IHttpContextAccessor httpContextAccessor,
                CancellationToken cancellationToken) =>
        {
            if (!httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
            {
                return Results.Unauthorized();
            }

            LogoutAllCommand command = new(RefreshToken: refreshToken);

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
        .WithTags("Logout All")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
