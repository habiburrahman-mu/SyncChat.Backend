using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.ResultHandling;
using SyncChat.API.Shared.Sender.Contracts;
using SyncChat.API.Shared.Socket.Contracts;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
    private readonly IHubContext<NotificationHub, INotificationClient> hub;
    private readonly IUserConnectionManager userConnectionManager;

    public SendMessageCommandHandler(
        ApplicationDbContext applicationDbContext,
        IHubContext<NotificationHub, INotificationClient> hub,
        IUserConnectionManager userConnectionManager)
    {
        dbContext = applicationDbContext;
        this.hub = hub;
        this.userConnectionManager = userConnectionManager;
    }
    public async Task<Result<SendMessageResponse>> HandleAsync(SendMessageCommand command, CancellationToken cancellationToken)
    {
        await dbContext.Database.BeginTransactionAsync(cancellationToken);

        Message message = await SaveMessageAsync(command, cancellationToken);

        // Update last message reference
        await dbContext.Conversations
            .Where(c => c.ConversationId == command.ConversationId)
            .ExecuteUpdateAsync(c => c.SetProperty(p => p.LastMessageId, message.MessageId), cancellationToken);

        User sender = await dbContext.Users
            .AsNoTracking()
            .FirstAsync(u => u.UserID == command.SenderId, cancellationToken);

        SendMessageResponse response = new(
            MessageId: message.MessageId,
            Uuid: message.Uuid,
            ConversationId: message.ConversationId,
            SenderId: message.SenderId,
            Type: message.Type,
            Content: message.Content,
            SenderUserName: sender.UserName,
            SenderByName: sender.Name,
            UpdatedAt: message.UpdatedAt,
            MetaData: message.MetaData,
            ReplyTo: message.ReplyTo);

        await dbContext.Database.CommitTransactionAsync(cancellationToken);

        await SendNotificationAsync(message, cancellationToken);

        return response;
    }

    private async Task<Message> SaveMessageAsync(SendMessageCommand command, CancellationToken cancellationToken)
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
        return message;
    }

    private async Task SendNotificationAsync(Message message, CancellationToken cancellationToken)
    {
        User? sender = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == message.SenderId, cancellationToken);

        if (sender is not null) message.Sender = sender;

        IReadOnlyList<string> sendersConnections = userConnectionManager.GetConnections(message.SenderId.ToString());

        await this.hub.Clients.GroupExcept(message.ConversationId.ToString(), sendersConnections)
            .MessageReceived(message.ToDTO());

        List<long> conversationMembers = await dbContext.ConversationMembers
            .AsNoTracking()
            .Where(x => x.ConversationId == message.ConversationId
                    && x.IsActive && x.LeftAt == null
                        && x.UserId != message.SenderId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);

        var notificationTasks = conversationMembers.Select(userId => this.hub.Clients.User(userId.ToString()).HasNewMessage(message.ConversationId));

        await Task.WhenAll(notificationTasks);
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