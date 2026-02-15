using SyncChat.API.Shared.Notification.Contracts.Models;

namespace SyncChat.API.Shared.Notification.Contracts;

public interface IMessageNotificationService
{
    Task NotifyMessageCreatedAsync(MessageNotificationModel message, CancellationToken cancellationToken);
}
