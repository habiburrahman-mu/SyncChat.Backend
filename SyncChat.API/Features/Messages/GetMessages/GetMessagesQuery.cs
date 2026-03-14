using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Storage.Contracts;

namespace SyncChat.API.Features.Messages.GetMessages;

public sealed record GetMessagesQuery(
    long ConversationID,
    long? LastMessageID = null,
    int PageSize = 20)
    : IQuery<GetMessagesResponse>;

public sealed class GetMessagesQueryHandler(
    ApplicationDbContext applicationDbContext,
    IIdentityService identityService,
    IBlobStorage blobStorage)
    : IQueryHandler<GetMessagesQuery, GetMessagesResponse>
{
    public async Task<Result<GetMessagesResponse>> HandleAsync(GetMessagesQuery query, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        // check if the user has access to the conversation
        bool isMember = await applicationDbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == query.ConversationID
                && cm.UserId == currentUserId);

        if (!isMember)
            return Result.Failure<GetMessagesResponse>(ConversationErrors.NotAuthorized(query.ConversationID));

        var messagesQuery = applicationDbContext.Messages
            .Where(m => m.ConversationId == query.ConversationID && m.DeletedAt == null);

        // Pagination filter: only older messages
        if (query.LastMessageID.HasValue)
        {
            messagesQuery = messagesQuery.Where(m => m.MessageId < query.LastMessageID.Value);
        }

        var entities = await messagesQuery
            .OrderByDescending(m => m.MessageId)
            .Include(m => m.Sender)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        entities.Reverse();

        var messages = entities.Select(m => m.ToDTO(blobStorage.GetPublicObjectUrl)).ToList();

        return new GetMessagesResponse(messages);
    }
}
