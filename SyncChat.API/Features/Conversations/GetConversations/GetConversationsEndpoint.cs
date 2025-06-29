
namespace SyncChat.API.Features.Conversations.GetConversations;

public sealed class GetConversationsEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        //group.MapGet(ConversationRoute.GetConversations,
        //    async (IQuerySender sender, CancellationToken cancellationToken) =>
        //    {
        //        GetConversationsQuery query = new();
        //        Result<List<GetConversationsResponse>> result = await sender.SendAsync(query, cancellationToken);
        //        return result.Match(
        //            conversations => Results.Ok(conversations),
        //            CustomResults.Problem);
        //    })
        //    .WithSummary("Get Conversations")
        //    .Produces<List<GetConversationsResponse>>(StatusCodes.Status200OK);
    }
}
