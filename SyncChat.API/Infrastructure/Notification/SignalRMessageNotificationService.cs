using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Features.Notifications;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Notification.Contracts;
using SyncChat.API.Shared.Notification.Contracts.Models;
using SyncChat.API.Shared.Socket.Contracts;

namespace SyncChat.API.Infrastructure.Notification;

public sealed class SignalRMessageNotificationService : IMessageNotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> hub;
    private readonly IUserConnectionManager userConnectionManager;
    private readonly ApplicationDbContext dbContext;
    private readonly ILogger<SignalRMessageNotificationService> logger;

    public SignalRMessageNotificationService(
        IHubContext<NotificationHub, INotificationClient> hub,
        IUserConnectionManager userConnectionManager,
        ApplicationDbContext dbContext,
        ILogger<SignalRMessageNotificationService> logger)
    {
        this.hub = hub;
        this.userConnectionManager = userConnectionManager;
        this.dbContext = dbContext;
        this.logger = logger;
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

        var notificationTasks = conversationMembers.Select(userId => hub.Clients.User(userId.ToString()).HasNewMessage(message.ConversationId));

        await Task.WhenAll(notificationTasks);
    }

    public async Task NotifyMembersAddedAsync(MembersAddedNotificationModel model, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .FirstAsync(c => c.ConversationId == model.ConversationId, cancellationToken);

        ConversationDTO conversationDTO = conversation.ToDTO()!;

        var allNotificationTasks = model.NewMemberIds
            .Select(userId => hub.Clients.User(userId.ToString()).AddedToConversation(conversationDTO))
            .Concat(model.ExistingMemberIds.Select(userId => hub.Clients.User(userId.ToString()).HasNewMessage(model.ConversationId)))
            .Concat(model.SystemMessages.Select(message => hub.Clients.Groups(message.ConversationId.ToString()).MessageReceived(message)))
            .Concat(model.ExistingMemberIds.Select(userId => hub.Clients.User(userId.ToString()).NewMemberAdded(model.ConversationId)));

        await Task.WhenAll(allNotificationTasks);
    }

    public async Task NotifyMemberRemovedAsync(MemberRemovedNotificationModel model, CancellationToken cancellationToken)
    {
        try
        {
            List<long> remainingMembers = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(x => x.ConversationId == model.ConversationId && x.IsActive && x.LeftAt == null)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            var allNotificationTasks = new[]
            {
                hub.Clients.User(model.RemovedMemberId.ToString()).RemovedFromConversation(model.ConversationId)
            }
            .Concat(remainingMembers.Select(userId => hub.Clients.User(userId.ToString()).HasNewMessage(model.ConversationId)))
            .Concat([hub.Clients.Groups(model.ConversationId.ToString()).MessageReceived(model.SystemMessage)])
            .Concat(remainingMembers.Select(userId => hub.Clients.User(userId.ToString()).MemberRemoved(model.ConversationId)));

            await Task.WhenAll(allNotificationTasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send member removed notification to conversation {ConversationId}", model.ConversationId);
        }
    }

    public async Task NotifyMemberPromotedAsync(MemberPromotedNotificationModel model, CancellationToken cancellationToken)
    {
        try
        {
            List<long> memberIds = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(x => x.ConversationId == model.ConversationId && x.IsActive && x.LeftAt == null)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            var allNotificationTasks = new[]
            {
                hub.Clients.User(model.PromotedMemberId.ToString()).PromotedToAdmin(model.ConversationId),
                hub.Clients.Groups(model.ConversationId.ToString()).MessageReceived(model.SystemMessage)
            }
            .Concat(memberIds.Select(userId => hub.Clients.User(userId.ToString()).MemberRoleChanged(model.ConversationId)))
            .Concat(memberIds.Select(userId => hub.Clients.User(userId.ToString()).HasNewMessage(model.ConversationId)));

            await Task.WhenAll(allNotificationTasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send member promoted notification to conversation {ConversationId}", model.ConversationId);
        }
    }

    public async Task NotifyMemberDemotedAsync(MemberDemotedNotificationModel model, CancellationToken cancellationToken)
    {
        try
        {
            List<long> activeUsers = await dbContext.ConversationMembers
                .AsNoTracking()
                .Where(m => m.ConversationId == model.ConversationId && m.IsActive && m.LeftAt == null)
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);

            var allNotificationTasks = new[]
            {
                hub.Clients.User(model.DemotedMemberId.ToString()).MemberRoleChanged(model.ConversationId),
                hub.Clients.Groups(model.ConversationId.ToString()).MessageReceived(model.SystemMessage)
            }
            .Concat(activeUsers.Select(uid => hub.Clients.User(uid.ToString()).HasNewMessage(model.ConversationId)))
            .Concat(activeUsers.Select(uid => hub.Clients.User(uid.ToString()).MemberDemoted(model.ConversationId)));

            await Task.WhenAll(allNotificationTasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed sending demotion notifications in conversation {ConversationId}", model.ConversationId);
        }
    }

    public async Task NotifyConversationCreatedAsync(ConversationCreatedNotificationModel model, CancellationToken cancellationToken)
    {
        var notificationTasks = model.OtherMemberIds
            .Select(userId => hub.Clients.User(userId.ToString()).NewConversationCreated(model.Conversation));

        await Task.WhenAll(notificationTasks);
    }
}
