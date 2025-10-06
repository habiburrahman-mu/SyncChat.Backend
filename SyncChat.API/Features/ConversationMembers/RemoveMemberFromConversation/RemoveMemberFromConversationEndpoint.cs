using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.RemoveMemberFromConversation;

public sealed class RemoveMemberFromConversationEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapDelete(
            ConversationMemberRoute.Remove + "/{conversationMemberId:long}",
            async ([FromRoute] long conversationMemberId,
            ICommandSender sender,
            CancellationToken cancellationToken) =>
            {
                RemoveConversationMemberCommand command = new(conversationMemberId);
                
                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithSummary("Remove conversation member")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
