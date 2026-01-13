using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.GetConversationDetail;

public sealed record GetConversationDetailQuery(long ConversationId)
    : IQuery<ConversationDTO>;

public sealed class GetConversationDetailQueryHandler(IIdentityService identityService, ApplicationDbContext dbContext)
    : IQueryHandler<GetConversationDetailQuery, ConversationDTO>
{
    public async Task<Result<ConversationDTO>> HandleAsync(GetConversationDetailQuery query, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        bool isMember = await dbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == query.ConversationId && cm.UserId == currentUserId, cancellationToken);

        if (!isMember)
            return Result.Failure<ConversationDTO>(ConversationErrors.NotAuthorized(query.ConversationId));

        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Where(c => c.ConversationId == query.ConversationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation == null)
            return Result.Failure<ConversationDTO>(ConversationErrors.NotFound(query.ConversationId));

        return conversation.ToDTO();
    }
}