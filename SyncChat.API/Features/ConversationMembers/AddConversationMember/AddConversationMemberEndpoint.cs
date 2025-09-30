
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.ConversationMembers.AddConversationMember;

public sealed record AddConversationMemberRequest(
    long ConversationId,
    List<long> MemberIds);

public sealed class AddConversationMemberEndpoint : IConversationMemberEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(ConversationMemberRoute.Add,
            async ([FromBody] AddConversationMemberRequest request, ICommandSender commandSender, CancellationToken cancellationToken) =>
            {
                AddConversationMemberCommand command = new(
                    ConversationId: request.ConversationId,
                    MemberIds: request.MemberIds);

                Result result = await commandSender.SendAsync(command, cancellationToken);
                return result.Match(Results.Created, CustomResults.Problem);
            })
            .WithSummary("Add Conversation Member")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
