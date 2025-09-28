using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.ConversationMembers.GetConversationMembers;

public sealed record GetConversationMembersQuery(long ConversationId) : IQuery<List<ConversationMemberDTO>>;

public sealed class GetConversationMembersQueryHandler(IIdentityService identityService, ApplicationDbContext dbContext)
    : IQueryHandler<GetConversationMembersQuery, List<ConversationMemberDTO>>
{
    public async Task<Result<List<ConversationMemberDTO>>> HandleAsync(GetConversationMembersQuery query, CancellationToken cancellationToken = default)
    {
        {
            long currentUserId = identityService.GetUserID();

            bool isMember = await dbContext.ConversationMembers
               .AnyAsync(cm => cm.ConversationId == query.ConversationId && cm.UserId == currentUserId, cancellationToken);

            if (!isMember)
                return Result.Failure<List<ConversationMemberDTO>>(ConversationErrors.NotAuthorized(query.ConversationId));

            bool conversationExists = await dbContext.Conversations
                .AsNoTracking()
                .AnyAsync(c => c.ConversationId == query.ConversationId, cancellationToken);

            if (!conversationExists)
                return Result.Failure<List<ConversationMemberDTO>>(ConversationErrors.NotFound(query.ConversationId));

            var members = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(cm => cm.ConversationId == query.ConversationId)
                .OrderByDescending(cm => cm.User.UserID == currentUserId)
                .ThenBy(cm => cm.User.Name)
                .Select(cm => new ConversationMemberDTO
                {
                    UserID = cm.User.UserID,
                    UserName = cm.User.UserName,
                    Name = cm.User.Name,
                    Role = cm.Role,
                    JoinedAt = cm.JoinedAt,
                    IsActive = cm.IsActive,
                    LeftAt = cm.LeftAt
                })
                .ToListAsync(cancellationToken);

            return members;
        }
    }
}