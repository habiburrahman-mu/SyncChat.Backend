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

            var dtos = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(cm => cm.UserId == userId && cm.IsActive)
                .OrderByDescending(cm =>
                     cm.Conversation.LastMessage != null
                        ? cm.Conversation.LastMessage.UpdatedAt
                        : cm.Conversation.UpdatedAt)
                .Select(cm => new ConversationDTO
                {
                    ConversationId = cm.ConversationId,
                    Uuid = cm.Conversation.Uuid,
                    Type = cm.Conversation.Type,
                    AvatarUrl = cm.Conversation.AvatarUrl,
                    Settings = cm.Conversation.Settings,
                    CreatedBy = cm.Conversation.CreatedBy,
                    CreatedAt = cm.Conversation.CreatedAt,
                    UpdatedAt = cm.Conversation.UpdatedAt,

                    // get last message text or empty
                    LastMessageId = cm.Conversation.LastMessageId,
                    LastMessage = cm.Conversation.LastMessage != null
                                        ? cm.Conversation.LastMessage.Content
                                        : null,
                    LastMessageMetaData = cm.Conversation.LastMessage != null
                                        ? cm.Conversation.LastMessage.MetaData
                                        : null,

                    // for direct chats, pick the OTHER member's name; else keep existing name
                    Name = (cm.Conversation.Type == ConversationType.Direct
                        ? cm.Conversation.Members
                            .Where(m => m.UserId != userId)
                            .Select(m => m.User.Name)
                            .FirstOrDefault()
                        : cm.Conversation.Name)!,

                    OtherUserId = cm.Conversation.Type == ConversationType.Direct
                        ? cm.Conversation.Members
                              .Where(m => m.UserId != userId)
                              .Select(m => m.UserId)
                              .FirstOrDefault()
                        : null,

                    LastSeenMessageId = cm.LastSeenMessageId,

                    HaveUnreadMessages = cm.Conversation.LastMessageId != null ? (cm.LastSeenMessageId ?? 0) < cm.Conversation.LastMessageId : false
                })
                .ToListAsync(cancellationToken);


            return new GetConversationsResponse(dtos);
        }

        return Result.Failure<GetConversationsResponse>(ConversationErrors.InvalidUser(userId));
    }
}
