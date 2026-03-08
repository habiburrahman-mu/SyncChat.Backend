using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record MemberDemotedNotificationModel(
    long ConversationId,
    long DemotedMemberId,
    MessageDTO SystemMessage);
