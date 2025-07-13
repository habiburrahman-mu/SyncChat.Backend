using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Messages.GetMessages;

public sealed record GetMessagesQuery(long ConversationID)
    : IQuery<GetMessagesResponse>;

public sealed class GetMessagesQueryHandler(ApplicationDbContext applicationDbContext, IIdentityService identityService)
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


        List<MessageDTO> conversations = await applicationDbContext.Messages
            .Where(m =>
                m.ConversationId == query.ConversationID
                && m.DeletedAt == null)
            .Include(m => m.Sender)
            .Select(m => m.ToDTO())
            .ToListAsync(cancellationToken);

        return new GetMessagesResponse(conversations);
    }
}