
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
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


public sealed class SendMessageEndpoint : IMessageEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(MessageRoute.Send,
            async ([FromBody] SendMessageRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                SendMessageCommand command = new(
                    ConversationId: request.ConversationId,
                    SenderId: request.SenderId,
                    Type: request.Type,
                    Content: request.Content,
                    MetaData: request.MetaData,
                    ReplyTo: request.ReplyTo);

                Result<SendMessageResponse> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(
                    response => Results.Created("", response),
                    CustomResults.Problem);
            })
            .WithSummary("Send Message")
            .Produces<SendMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
