
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.GetConversationMembers;

public class GetConversationMembersEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(ConversationMemberRoute.GetList + "/{conversationId}",
            async ([FromRoute] long conversationId, IQuerySender querySender, CancellationToken cancellationToken) =>
            {
                var query = new GetConversationMembersQuery(ConversationId: conversationId);

                var result = await querySender.SendAsync(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithSummary("Get Conversation Members by Conversation ID")
            .Produces<List<ConversationMemberDTO>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
