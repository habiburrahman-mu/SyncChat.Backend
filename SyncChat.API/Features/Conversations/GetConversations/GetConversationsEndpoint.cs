using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.GetConversations;

public sealed class GetConversationsEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(ConversationRoute.GetList,
            async (IQuerySender sender, CancellationToken cancellationToken) =>
            {
                GetConversationsQuery query = new();
                Result<GetConversationsResponse> result = await sender.SendAsync(query, cancellationToken);
                return result.Match(
                    conversations => Results.Ok(conversations),
                    CustomResults.Problem);
            })
            .WithSummary("Get Conversations")
            .Produces<GetConversationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
