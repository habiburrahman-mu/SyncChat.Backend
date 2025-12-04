
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.MakeMemberAdmin;

public class MakeMemberAdminEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(
            ConversationMemberRoute.MakeAdmin + "/{conversationMemberId:long}",
            async ([FromRoute] long conversationMemberId,
            ICommandSender sender,
            CancellationToken cancellationToken) =>
            {
                MakeMemberAdminCommand command = new(conversationMemberId);

                Result result = await sender.SendAsync(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithSummary("Make a member admin");
    }
}
