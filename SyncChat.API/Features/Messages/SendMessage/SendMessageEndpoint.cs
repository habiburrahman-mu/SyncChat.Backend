
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Messages.SendMessage;

public sealed record SendMessageRequest(
    long ConversationId,
    long SenderId,
    MessageType Type,
    string? Content,
    string MetaData = "{}",
    long? ReplyTo = null
);


public class SendMessageEndpoint : IMessageEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(MessageRoute.Send,
            async ([FromBody] SendMessageRequest request, IQuerySender sender, CancellationToken cancellationToken) =>
            {
                SendMessageCommand command = new(
                    ConversationID: request.ConversationId,
                    SenderID: request.SenderId,
                    Type: request.Type,
                    Content: request.Content,
                    MetaData: request.MetaData,
                    ReplyTo: request.ReplyTo);

                Result<long> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            });
    }
}
