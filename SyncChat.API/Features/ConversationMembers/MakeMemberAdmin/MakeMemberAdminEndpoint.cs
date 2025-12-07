
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.MakeMemberAdmin;

public sealed class MakeMemberAdminEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPut(
            ConversationMemberRoute.MakeAdmin + "/{conversationMemberId:long}",
            async ([FromRoute] long conversationMemberId,
            ICommandSender sender,
            CancellationToken cancellationToken) =>
            {
                MakeMemberAdminCommand command = new(conversationMemberId);

                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .WithSummary("Make a member admin");
    }
}
