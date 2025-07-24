using Microsoft.AspNetCore.SignalR;
using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Features.Notifications;

public interface INotificationClient
{
    Task ReceiveMessage(MessageDTO message);
}

public class NotificationHub : Hub<INotificationClient>
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
