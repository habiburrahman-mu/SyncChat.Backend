using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Messages.SendMessage;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Errors;
using SyncChat.API.Shared.Notification.Contracts;
using SyncChat.API.Shared.Notification.Contracts.Models;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;

namespace SyncChat.API.Features.Messages.SendMediaMessage;

public sealed record SendMediaMessageCommand(
    long ConversationId,
    long SenderId,
    MessageType Type,
    Guid MediaId,
    string? Caption,
    long? ReplyTo = null) : ICommand;


public sealed class SendMediaMessageCommandHandler : ICommandHandler<SendMediaMessageCommand>
{
    private readonly ApplicationDbContext dbContext;
    private readonly IMessageNotificationService messageNotificationService;

    public SendMediaMessageCommandHandler(ApplicationDbContext dbContext, IMessageNotificationService messageNotificationService)
    {
        this.dbContext = dbContext;
        this.messageNotificationService = messageNotificationService;
    }

    public async Task<Result> HandleAsync(SendMediaMessageCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        bool isMember = await dbContext.ConversationMembers
           .AnyAsync(x =>
               x.ConversationId == command.ConversationId &&
               x.UserId == command.SenderId &&
               x.IsActive &&
               x.LeftAt == null,
               cancellationToken);

        if (!isMember)
            return Result.Failure(ConversationMemberErrors.Forbidden());

        var media = await dbContext.Media
            .FirstOrDefaultAsync(m => m.Id == command.MediaId, cancellationToken);

        if (media is null)
            return Result.Failure(MediaErrors.NotFound(command.MediaId));

        if (media.State != MediaState.Active)
            return Result.Failure(MediaErrors.MediaIsNotActive);

        if (media.OwnerType != MediaOwnerType.Conversation ||
            media.OwnerId != command.ConversationId.ToString())
            return Result.Failure(MediaErrors.Forbidden);

        var message = new Message
        {
            Uuid = Guid.NewGuid(),
            ConversationId = command.ConversationId,
            SenderId = command.SenderId,
            Type = command.Type,
            Content = command.Caption,
            MediaId = media.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await dbContext.Messages.AddAsync(message, cancellationToken);

        await dbContext.MessageStatuses.AddAsync(new MessageStatus
        {
            Message = message,
            UserId = message.SenderId,
            Status = DeliveryStatus.Sent,
            UpdatedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Conversations
            .Where(c => c.ConversationId == command.ConversationId)
            .ExecuteUpdateAsync(c =>
                c.SetProperty(p => p.LastMessageId, message.MessageId),
                cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        User sender = await dbContext.Users
            .AsNoTracking()
            .FirstAsync(u => u.UserID == command.SenderId, cancellationToken);

        await SendNotificationAsync(message, sender, cancellationToken);

        return Result.Success();
    }

    private async Task SendNotificationAsync(Message message, User sender, CancellationToken cancellationToken)
    {
        MessageNotificationModel notificationModel = new(
                    MessageId: message.MessageId,
                    Uuid: message.Uuid,
                    ConversationId: message.ConversationId,
                    SenderId: message.SenderId,
                    SenderUserName: sender.UserName,
                    SenderName: sender.Name,
                    Type: message.Type,
                    Content: message.Content,
                    MediaId: message.MediaId,
                    MetaData: message.MetaData,
                    ReplyTo: message.ReplyTo,
                    CreatedAt: message.CreatedAt,
                    UpdatedAt: message.UpdatedAt);

        await messageNotificationService.NotifyMessageCreatedAsync(notificationModel, cancellationToken);
    }
}
public sealed class SendMediaMessageCommandValidator : AbstractValidator<SendMediaMessageCommand>
{
    public SendMediaMessageCommandValidator()
    {

        RuleFor(x => x.ConversationId)
            .GreaterThan(0).WithMessage("Conversation ID must be greater than 0.");

        RuleFor(x => x.SenderId).GreaterThan(0).WithMessage("Sender ID must be greater than 0.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid message type.");
    }
}