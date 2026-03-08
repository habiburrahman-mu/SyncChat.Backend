using SyncChat.API.Features.Conversations.DTOs;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record ConversationCreatedNotificationModel(
    IReadOnlyList<long> OtherMemberIds,
    ConversationDTO Conversation);
