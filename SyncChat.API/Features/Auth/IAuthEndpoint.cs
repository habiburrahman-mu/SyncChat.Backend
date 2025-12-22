using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Auth;

[RouteGroupPrefix(AuthRoute.Base, "Auth")]
public interface IAuthEndpoint : IEndpoint { }