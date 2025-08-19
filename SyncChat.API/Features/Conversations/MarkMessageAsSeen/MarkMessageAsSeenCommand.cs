using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.MarkMessageAsSeen;

public sealed record MarkMessageAsSeenCommand(
    long ConversationId,
    long MessageId) : ICommand;

public sealed class MarkMessageAsSeenCommandHandler(ApplicationDbContext dbContext, IIdentityService identityService)
    : ICommandHandler<MarkMessageAsSeenCommand>
{
    public async Task<Result> HandleAsync(MarkMessageAsSeenCommand command, CancellationToken cancellationToken = default)
    {
        long currentUserId = identityService.GetUserID();

        await dbContext.ConversationMembers
            .Where(x =>
                x.ConversationId == command.ConversationId
                && x.UserId == currentUserId
                && x.LeftAt == null
                && x.IsActive)
            .ExecuteUpdateAsync(x => x.SetProperty(m => m.LastSeenMessageId, command.MessageId));

        return Result.Success();
    }
}

public sealed class MarkMessageAsSeenCommandValidator : AbstractValidator<MarkMessageAsSeenCommand>
{
    public MarkMessageAsSeenCommandValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.MessageId).GreaterThan(0);
    }
}