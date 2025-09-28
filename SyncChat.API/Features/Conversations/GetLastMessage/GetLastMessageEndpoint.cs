using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.GetLastMessage;

public class GetLastMessageEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(ConversationRoute.GetLastMessage + "/{conversationId}",
            async ([FromRoute] long conversationId, IQuerySender querySender, CancellationToken cancellationToken) =>
            {
                var query = new GetLastMessageQuery(ConversationId: conversationId);

                var result = await querySender.SendAsync(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithSummary("Get Last Message of Conversation")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
