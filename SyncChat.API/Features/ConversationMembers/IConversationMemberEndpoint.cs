using SyncChat.API.Routing;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers;

[RouteGroupPrefix(ConversationMemberRoute.Base, "Conversation Members", HasAuthorization = true)]
public interface IConversationMemberEndpoint : IEndpoint { }
