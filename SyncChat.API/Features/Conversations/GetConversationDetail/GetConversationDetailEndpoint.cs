
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.GetConversationDetail;

public sealed class GetConversationDetailEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(ConversationRoute.GetDetail + "/{conversationId}",
            async ([FromRoute] long conversationId, IQuerySender querySender, CancellationToken cancellationToken) =>
            {
                var query = new GetConversationDetailQuery(ConversationId: conversationId);

                var result = await querySender.SendAsync(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithSummary("Get Conversation Detail by ID")
            .Produces<ConversationDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
