using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.RemoveMemberFromConversation;

public sealed record RemoveMemberFromConversationRequest(long ConverstionMemberId);

public sealed class RemoveMemberFromConversationEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapDelete(
            ConversationMemberRoute.Remove,
            async ([FromBody] RemoveMemberFromConversationRequest request,
            ICommandSender sender,
            CancellationToken cancellationToken) =>
            {
                RemoveConversationMemberCommand command = new(request.ConverstionMemberId);
                
                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithSummary("Remove conversation member")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
