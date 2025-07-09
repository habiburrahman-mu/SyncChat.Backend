using SyncChat.API.Features.Messages.DTOs;

namespace SyncChat.API.Features.Messages.GetMessages;

public sealed record GetMessagesResponse(List<MessageDTO> Messages);