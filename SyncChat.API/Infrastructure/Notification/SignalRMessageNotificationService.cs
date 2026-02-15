using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Notification.Contracts;
using SyncChat.API.Shared.Notification.Contracts.Models;
using SyncChat.API.Shared.Socket.Contracts;
using System.Reactive;

namespace SyncChat.API.Infrastructure.Notification;

public sealed class SignalRMessageNotificationService : IMessageNotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> hub;
    private readonly IUserConnectionManager userConnectionManager;
    private readonly ApplicationDbContext dbContext;

    public SignalRMessageNotificationService(
        IHubContext<NotificationHub, INotificationClient> hub,
        IUserConnectionManager userConnectionManager,
        ApplicationDbContext dbContext)
    {
        this.hub = hub;
        this.userConnectionManager = userConnectionManager;
        this.dbContext = dbContext;
    }

    public async Task NotifyMessageCreatedAsync(MessageNotificationModel message, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> senderConnections = userConnectionManager.GetConnections(message.SenderId.ToString());

        await hub.Clients
            .GroupExcept(message.ConversationId.ToString(), senderConnections)
            .MessageReceived(new MessageDTO
            {
                MessageId = message.MessageId,
                Uuid = message.Uuid,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Type = message.Type,
                Content = message.Content,
                MediaId = message.MediaId,
                MetaData = message.MetaData,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                SenderUserName = message.SenderUserName,
                SenderName = message.SenderName,
                ReplyTo = message.ReplyTo,
            });

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
