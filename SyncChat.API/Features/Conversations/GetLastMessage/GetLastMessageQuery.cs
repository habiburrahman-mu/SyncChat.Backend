using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.GetLastMessage;

public sealed record GetLastMessageQuery(long ConversationId) : IQuery<string>;

public sealed class GetLastMessageQueryHandler(
    ApplicationDbContext dbContext,
    IIdentityService identityService)
    : IQueryHandler<GetLastMessageQuery, string>
{
    public async Task<Result<string>> HandleAsync(GetLastMessageQuery query, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        bool isMember = await dbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == query.ConversationId && cm.UserId == currentUserId, cancellationToken);

        if (!isMember)
            return Result.Failure<string>(ConversationErrors.NotAuthorized(query.ConversationId));

        string? lastMessageContent = await dbContext.Conversations
            .Where(c => c.ConversationId == query.ConversationId)
            .Select(c => c.LastMessage!.Content)
            .FirstOrDefaultAsync(cancellationToken);

        return lastMessageContent is null
            ? Result.Failure<string>(ConversationErrors.LastMessageNotFound(query.ConversationId))
            : Result.Success(lastMessageContent);
    }
}