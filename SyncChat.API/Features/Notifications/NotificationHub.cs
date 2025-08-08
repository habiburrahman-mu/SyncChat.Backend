using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncChat.API.Features.Conversations.DTOs;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Shared.Socket.Contracts;

namespace SyncChat.API.Features.Notifications;

public interface INotificationClient
{
    /// <summary>
    /// Called by the server when a new message is received in the chat.
    /// </summary>
    Task MessageReceived(MessageDTO message);

    Task HasNewMessage(long conversationId);

    /// <summary>
    /// Called by the server when a new conversation is created or added for the client.
    /// </summary>
    Task NewConversationCreated(ConversationDTO conversation);
}

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private readonly IUserConnectionManager userConnectionManager;
    private readonly ApplicationDbContext dbContext;

    public NotificationHub(
        IUserConnectionManager userConnectionManager,
        ApplicationDbContext applicationDbContext)
    {
        this.userConnectionManager = userConnectionManager;
        dbContext = applicationDbContext;
    }

    public override Task OnConnectedAsync()
    {
        string userId = Context.UserIdentifier ?? throw new InvalidOperationException("User identifier is not set.");
        userConnectionManager.AddConnection(userId, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        string userIdString = Context.UserIdentifier ?? throw new InvalidOperationException("User identifier is not set.");

        if (!long.TryParse(groupName, out long groupId)) return;

        if (!long.TryParse(userIdString, out long userId)) return;

        var isAuthorized = await IsUserMemberOfConversation(userId, groupId);

        if (!isAuthorized) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string userId = Context.UserIdentifier ?? throw new InvalidOperationException("User identifier is not set.");
        userConnectionManager.RemoveConnection(userId, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> IsUserMemberOfConversation(long userId, long conversationId)
    {
        return await dbContext.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
    }
}
