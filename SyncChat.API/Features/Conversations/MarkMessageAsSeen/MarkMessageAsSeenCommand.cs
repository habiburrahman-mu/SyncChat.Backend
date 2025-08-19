using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Conversations.MarkMessageAsSeen;

public sealed record MarkMessageAsSeenCommand(
    long ConversationId,
    long UserId,
    long MessageId) : ICommand;

public sealed class MarkMessageAsSeenCommandHandler(ApplicationDbContext dbContext)
    : ICommandHandler<MarkMessageAsSeenCommand>
{
    public async Task<Result> HandleAsync(MarkMessageAsSeenCommand command, CancellationToken cancellationToken = default)
    {
        await dbContext.ConversationMembers
            .Where(x =>
                x.ConversationId == command.ConversationId
                && x.UserId == command.UserId
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
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.MessageId).GreaterThan(0);
    }
}