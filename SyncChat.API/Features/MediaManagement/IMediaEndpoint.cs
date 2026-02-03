using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.MediaManagement;

[RouteGroupPrefix(prefix: MediaRoute.Base, groupName: "Media", hasAuthorization: true)]
public interface IMediaEndpoint : IEndpoint { }
