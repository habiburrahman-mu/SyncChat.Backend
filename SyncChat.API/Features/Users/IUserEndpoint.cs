using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Users;

[RouteGroupPrefix(UserRoute.Base, "User", hasAuthorization: true)]
public interface IUserEndpoint : IEndpoint
{
}