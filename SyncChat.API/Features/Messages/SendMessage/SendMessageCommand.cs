using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Messages.SendMessage;

public sealed record SendMessageCommand(
    long ConversationId,
    long SenderId,
    MessageType Type,
    string? Content,
    string MetaData = "{}",
    long? ReplyTo = null) : ICommand<SendMessageResponse>;

public sealed class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly ApplicationDbContext dbContext;

    public SendMessageCommandHandler(ApplicationDbContext applicationDbContext)
    {
        dbContext = applicationDbContext;
    }
    public async Task<Result<SendMessageResponse>> HandleAsync(SendMessageCommand command, CancellationToken cancellationToken)
    {
        Message message = new()
        {
            Uuid = Guid.NewGuid(),
            ConversationId = command.ConversationId,
            SenderId = command.SenderId,
            Type = command.Type,
            Content = command.Content,
            MetaData = command.MetaData,
            ReplyTo = command.ReplyTo,
        };

        await dbContext.Messages.AddAsync(message, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        SendMessageResponse response = new(
            MessageId: message.MessageId,
            Uuid: message.Uuid,
            ConversationId: message.ConversationId,
            SenderId: message.SenderId,
            Type: message.Type,
            Content: message.Content,
            MetaData: message.MetaData,
            ReplyTo: message.ReplyTo);

        return response;
    }
}

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    private readonly ApplicationDbContext dbContext;

    public SendMessageCommandValidator(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.ConversationId)
            .GreaterThan(0).WithMessage("Conversation ID must be greater than 0.");

        RuleFor(x => x.SenderId).GreaterThan(0).WithMessage("Sender ID must be greater than 0.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid message type.");

        RuleFor(x => x.MetaData).MaximumLength(5000).WithMessage("MetaData cannot exceed 5,000 characters.");

        RuleFor(x => x)
            .MustAsync(IsUserMemberOfConversation)
            .WithMessage("Sender is not a member of the conversation.");
    }

    private async Task<bool> IsUserMemberOfConversation(SendMessageCommand command, CancellationToken cancellationToken)
    {
        return await dbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == command.ConversationId && cm.UserId == command.SenderId, cancellationToken);
    }
}