using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations;

[RouteGroupPrefix(UserRoute.Base, "User")]
public interface IConversationEndpoint : IEndpoint
{
}
