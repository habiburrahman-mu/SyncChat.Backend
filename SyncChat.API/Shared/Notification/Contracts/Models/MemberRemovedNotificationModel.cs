using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record MemberRemovedNotificationModel(
    long ConversationId,
    long RemovedMemberId,
    MessageDTO SystemMessage);
