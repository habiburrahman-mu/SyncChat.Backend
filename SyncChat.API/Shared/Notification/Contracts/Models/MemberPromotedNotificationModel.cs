using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record MemberPromotedNotificationModel(
    long ConversationId,
    long PromotedMemberId,
    MessageDTO SystemMessage);
