using SyncChat.API.Features.Conversations.DTOs;

namespace SyncChat.API.Features.Conversations.GetConversations;

public sealed record GetConversationsResponse(List<ConversationDTO> Conversations);