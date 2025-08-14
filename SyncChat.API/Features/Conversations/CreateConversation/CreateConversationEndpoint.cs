
using Microsoft.AspNetCore.Mvc;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using static SyncChat.API.Shared.Constants.EndpointConstants;

namespace SyncChat.API.Features.Conversations.CreateConversation;

public sealed class CreateConversationEndpoint : IConversationEndpoint
{
    public sealed record CreateConversationRequest(
        long CreatedBy,
        List<long> MemberIdList,
        string Name,
        ConversationType Type);

    public void Map(RouteGroupBuilder group)
    {
        group.MapPost(ConversationRoute.Create,
            async ([FromBody] CreateConversationRequest request, ICommandSender sender, CancellationToken cancellationToken) =>
            {
                CreateConversationCommand command = new(
                    request.CreatedBy,
                    request.MemberIdList,
                    request.Name,
                    request.Type);

                Result<long> result = await sender.SendAsync(command, cancellationToken);

                return result.Match(
                    conversationId => Results.Created("", conversationId),
                    CustomResults.Problem);
            })
            .WithSummary("Create Conversation")
            .RequireAuthorization()
            .Produces<Guid>(StatusCodes.Status201Created);
    }
}
