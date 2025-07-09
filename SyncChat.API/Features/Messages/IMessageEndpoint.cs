using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Messages;

[RouteGroupPrefix(MessageRoute.Base, "Message", HasAuthorization = true)]
public interface IMessageEndpoint : IEndpoint { }
