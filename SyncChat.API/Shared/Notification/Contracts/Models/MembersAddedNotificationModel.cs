using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Shared.Notification.Contracts.Models;

public sealed record MembersAddedNotificationModel(
    long ConversationId,
    IReadOnlyList<long> NewMemberIds,
    IReadOnlyList<long> ExistingMemberIds,
    IReadOnlyList<MessageDTO> SystemMessages);
