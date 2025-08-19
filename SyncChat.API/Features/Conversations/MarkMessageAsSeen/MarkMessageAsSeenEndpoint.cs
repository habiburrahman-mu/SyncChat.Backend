using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.MarkMessageAsSeen;

public sealed record MarkMessageAsSeenRequest(
        long ConversationId,
        long UserId,
        long MessageId);

public sealed class MarkMessageAsSeenEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPut(ConversationRoute.MarkMessageAsSeen,
            async ([FromBody] MarkMessageAsSeenRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                MarkMessageAsSeenCommand command = new(
                    request.ConversationId,
                    request.UserId,
                    request.MessageId);

                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(
                    Results.NoContent,
                    CustomResults.Problem);
            })
            .WithSummary("Mark Message As Seen")
            .Produces<Guid>(StatusCodes.Status204NoContent);
    }
}
