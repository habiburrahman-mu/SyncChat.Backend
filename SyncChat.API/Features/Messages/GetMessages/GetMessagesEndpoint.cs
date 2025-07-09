
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Messages.GetMessages;

public class GetMessagesEndpoint : IMessageEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet(MessageRoute.GetList + "{id}",
            async (long id, IQuerySender querySender, CancellationToken cancellationToken) =>
            {
                GetMessagesQuery query = new(ConversationID: id);

                Result<GetMessagesResponse> result = await querySender.SendAsync(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            });
    }
}
