using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SyncChat.API.Features.Messages.DTOs;
using SyncChat.API.Shared.Socket.Contracts;

namespace SyncChat.API.Features.Notifications;

public interface INotificationClient
{
    Task ReceiveMessage(MessageDTO message);
}

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private readonly IUserConnectionManager userConnectionManager;

    public NotificationHub(IUserConnectionManager userConnectionManager)
    {
        this.userConnectionManager = userConnectionManager;
    }

    public override Task OnConnectedAsync()
    {
        string userId = Context.UserIdentifier ?? throw new InvalidOperationException("User identifier is not set.");
        userConnectionManager.AddConnection(userId, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
