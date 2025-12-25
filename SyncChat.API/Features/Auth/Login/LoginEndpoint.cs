using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth.Login;

public sealed record LoginRequest(string UserName, string Password, string DeviceIdentifier);

public sealed class LoginEndpoint : IAuthEndpoint
{

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoute.Login,
            async ([FromBody] LoginRequest request, ICommandSender sender,
            IHttpContextAccessor httpContextAccessor, IOptions<JWTSettings> jwtSettings,
            CancellationToken cancellationToken) =>
        {
            LoginCommand command = new(request.UserName, request.Password, request.DeviceIdentifier);

            Result<LoginResponse> result = await sender.SendAsync(command, cancellationToken);

            return result.Match(
                (token) =>
                {
                    var httpContext = httpContextAccessor.HttpContext!;

                    var jwtSettingsValues = jwtSettings.Value;

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddMinutes(jwtSettingsValues.RefreshTokenExpirationInMinutes)
                    };

                    httpContext.Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);

                    return Results.Ok(token.AccessToken);
                },
                CustomResults.Problem);
        })
        .WithSummary("Login")
        .Produces<string>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
