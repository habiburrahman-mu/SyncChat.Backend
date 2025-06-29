
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.SendMessage;

public record SendMessageRequest(Guid ConversationId, Guid UserId, string Message);

public class SendMessageEndpoint : IConversationEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        //group.MapPost(ConversationRoute.SendMessage, 
        //    async ([FromBody] SendMessageRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
        //    {
        //        SendMessageCommand command = new(request.ConversationId, request.UserId, request.Message);
        //        Result result = await sender.SendAsync(command, cancellationToken);
        //        return result.Match(
        //            _ => Results.Ok(),
        //            CustomResults.Problem);
        //    })
        //    .WithSummary("Send Message")
        //    .Produces(StatusCodes.Status201Created);
    }
}
