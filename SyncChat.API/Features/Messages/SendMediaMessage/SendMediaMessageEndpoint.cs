using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Messages.SendMediaMessage;

public sealed record SendMediaMessageRequest(
    long ConversationId,
    long SenderId,
    MessageType Type,
    Guid MediaId,
    string? Caption,
    long? ReplyTo = null
);

public sealed class SendMediaMessageEndpoint : IMessageEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(MessageRoute.SendMedia,
            async ([FromBody] SendMediaMessageRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                SendMediaMessageCommand command = new(
                    ConversationId: request.ConversationId,
                    SenderId: request.SenderId,
                    Type: request.Type,
                    MediaId: request.MediaId,
                    Caption: request.Caption,
                    ReplyTo: request.ReplyTo);

                Result<SendMediaMessageResponse> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(
                    response => Results.Created("", response),
                    CustomResults.Problem);
            })
            .WithSummary("Send Media Message")
            .Produces<SendMediaMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
