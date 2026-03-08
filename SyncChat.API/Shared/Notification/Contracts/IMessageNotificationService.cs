using SyncChat.API.Shared.Notification.Contracts.Models;

namespace SyncChat.API.Shared.Notification.Contracts;

public interface IMessageNotificationService
{
    Task NotifyMessageCreatedAsync(MessageNotificationModel message, CancellationToken cancellationToken);
    Task NotifyMembersAddedAsync(MembersAddedNotificationModel model, CancellationToken cancellationToken);
    Task NotifyMemberRemovedAsync(MemberRemovedNotificationModel model, CancellationToken cancellationToken);
    Task NotifyMemberPromotedAsync(MemberPromotedNotificationModel model, CancellationToken cancellationToken);
    Task NotifyMemberDemotedAsync(MemberDemotedNotificationModel model, CancellationToken cancellationToken);
    Task NotifyConversationCreatedAsync(ConversationCreatedNotificationModel model, CancellationToken cancellationToken);
}
