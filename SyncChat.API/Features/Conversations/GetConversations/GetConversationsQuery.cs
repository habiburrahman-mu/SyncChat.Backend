using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.GetConversations;

public sealed record GetConversationsQuery() : IQuery<GetConversationsResponse>;

public sealed class GetConversationsQueryHandler : IQueryHandler<GetConversationsQuery, GetConversationsResponse>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IIdentityService identityService;

    public GetConversationsQueryHandler(ApplicationDbContext applicationDbContext, IIdentityService identityService)
    {
        this.dbContext = applicationDbContext;
        this.identityService = identityService;
    }

    public async Task<Result<GetConversationsResponse>> HandleAsync(GetConversationsQuery query, CancellationToken cancellationToken = default)
    {
        long userId = identityService.GetUserID();

        if (userId > 0)
        {
            List<Conversation> conversations = await dbContext.ConversationMembers
                .Where(cm => cm.UserId == userId && cm.IsActive)
                .Include(cm => cm.Conversation)
                .ThenInclude(c => c.LastMessage)
                .OrderByDescending(x =>
                    x.Conversation.LastMessage != null ? x.Conversation.LastMessage!.UpdatedAt : x.Conversation.UpdatedAt)
                .Select(cm => cm.Conversation)
                .ToListAsync(cancellationToken);

            List<long> directConversationIds = conversations
                .Where(x => x.Type == ConversationType.Direct)
                .Select(x => x.ConversationId)
                .ToList();

            var conversationNames = await dbContext
                .ConversationMembers
                .Where(x => x.UserId != userId && directConversationIds.Contains(x.ConversationId))
                .Include(x => x.User)
                .Select(x => new { ConversationID = x.ConversationId, Name = x.User.Name })
                .ToListAsync(cancellationToken);

            List<ConversationDTO> conversationDTOs = conversations.ToDTOs();

            conversationDTOs.ForEach(conv =>
            {
                if (conv.Type == ConversationType.Direct)
                {
                    var name = conversationNames.Find(x => x.ConversationID == conv.ConversationId)?.Name;

                    if (name is not null)
                    {
                        conv.Name = name;
                    }
                }
            });

            return new GetConversationsResponse(conversationDTOs);
        }

        return Result.Failure<GetConversationsResponse>(ConversationErrors.InvalidUser(userId));
    }
}
