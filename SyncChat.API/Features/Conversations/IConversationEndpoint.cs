using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations;

[RouteGroupPrefix(ConversationRoute.Base, "Conversation", HasAuthorization = true)]
public interface IConversationEndpoint : IEndpoint { }
