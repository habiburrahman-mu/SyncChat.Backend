using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.ConversationMembers.GetConversationMembers;

public sealed record GetConversationMembersQuery(long ConversationId) : IQuery<List<ConversationMemberDTO>>;

public sealed class GetConversationMembersQueryHandler(
    IIdentityService identityService,
    ApplicationDbContext dbContext,
    IBlobStorage blobStorage)
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

            var raw = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(cm => cm.ConversationId == query.ConversationId)
                .OrderByDescending(cm => cm.User.UserID == currentUserId)
                .ThenBy(cm => cm.User.Name)
                .Select(cm => new
                {
                    ConversationMemberId = cm.MemberId,
                    cm.User.UserID,
                    cm.User.UserName,
                    cm.User.Name,
                    cm.User.AvatarKey,
                    cm.Role,
                    cm.JoinedAt,
                    cm.IsActive,
                    cm.LeftAt
                })
                .ToListAsync(cancellationToken);

            var members = raw.Select(cm => new ConversationMemberDTO
            {
                ConversationMemberId = cm.ConversationMemberId,
                UserID = cm.UserID,
                UserName = cm.UserName,
                Name = cm.Name,
                AvatarUrl = cm.AvatarKey != null ? blobStorage.GetPublicObjectUrl(cm.AvatarKey) : null,
                Role = cm.Role,
                JoinedAt = cm.JoinedAt,
                IsActive = cm.IsActive,
                LeftAt = cm.LeftAt
            }).ToList();

            return members;
        }
    }
}
