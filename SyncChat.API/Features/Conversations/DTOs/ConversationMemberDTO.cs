using SyncChat.API.Shared.Entities;

namespace SyncChat.API.Features.Conversations.DTOs;

public class ConversationMemberDTO
{
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MemberRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set;}
}