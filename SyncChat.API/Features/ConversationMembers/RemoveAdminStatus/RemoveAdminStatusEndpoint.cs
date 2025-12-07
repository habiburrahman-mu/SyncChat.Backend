using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.RemoveAdminStatus;

public sealed class RemoveAdminStatusEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPut(
            ConversationMemberRoute.RemoveAdminStatus + "/{conversationMemberId:long}",
            async ([FromRoute] long conversationMemberId,
                   ICommandSender sender,
                   CancellationToken cancellationToken) =>
            {
                RemoveAdminStatusCommand command = new(conversationMemberId);

                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .WithSummary("Remove admin role from a conversation member");
    }
}
