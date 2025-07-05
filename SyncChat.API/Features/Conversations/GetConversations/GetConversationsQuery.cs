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
                .Select(cm => cm.Conversation)
                .ToListAsync(cancellationToken);

            List<ConversationDTO> conversationDTOs = conversations.ToDTOs();

            return new GetConversationsResponse(conversationDTOs);
        }

        return Result.Failure<GetConversationsResponse>(ConversationErrors.InvalidUser(userId));
    }
}
