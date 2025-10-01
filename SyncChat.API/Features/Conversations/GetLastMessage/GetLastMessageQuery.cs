using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.GetLastMessage;

public sealed record GetLastMessageQuery(long ConversationId) : IQuery<GetLastMessageResponse>;

public sealed class GetLastMessageQueryHandler(
    ApplicationDbContext dbContext,
    IIdentityService identityService)
    : IQueryHandler<GetLastMessageQuery, GetLastMessageResponse>
{
    public async Task<Result<GetLastMessageResponse>> HandleAsync(GetLastMessageQuery query, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        bool isMember = await dbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == query.ConversationId && cm.UserId == currentUserId, cancellationToken);

        if (!isMember)
            return Result.Failure<GetLastMessageResponse>(ConversationErrors.NotAuthorized(query.ConversationId));

        var lastMessageContent = await dbContext.Conversations
            .Where(c => c.ConversationId == query.ConversationId)
            .Select(c => new
            {
                c.LastMessageId,
                Content = c.LastMessage != null ? c.LastMessage.Content : null,
                MetaData = c.LastMessage != null ? c.LastMessage.MetaData : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        return lastMessageContent != null && lastMessageContent.LastMessageId.HasValue
            ? Result.Success(new GetLastMessageResponse(
                lastMessageContent.LastMessageId.Value,
                lastMessageContent.Content,
                lastMessageContent.MetaData))
            : Result.Failure<GetLastMessageResponse>(ConversationErrors.LastMessageNotFound(query.ConversationId));
    }
}