using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
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

        var lastMessage = await dbContext.Conversations
            .Where(c => c.ConversationId == query.ConversationId)
            .Select(c => new LastMessageProjection(
                c.LastMessageId, 
                c.LastMessage != null ? c.LastMessage.Content : null, 
                c.LastMessage != null ? c.LastMessage.MetaData : null, 
                c.LastMessage != null ? c.LastMessage.Type : null))
            .FirstOrDefaultAsync(cancellationToken);

        return lastMessage != null && lastMessage.LastMessageId.HasValue
            ? Result.Success(new GetLastMessageResponse(
                lastMessage.LastMessageId.Value,
                lastMessage.Content,
                lastMessage.MessageType,
                lastMessage.MetaData))
            : Result.Failure<GetLastMessageResponse>(ConversationErrors.LastMessageNotFound(query.ConversationId));
    }
}

public sealed record LastMessageProjection(
    long? LastMessageId, 
    string? Content, 
    string? MetaData, 
    MessageType? MessageType);
