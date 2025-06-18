namespace SyncChat.API.Shared.Entities;

public class ConversationMember
{
    public long MemberId { get; set; }
    public long ConversationId { get; set; }
    public long UserId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LeftAt { get; set; }
    public string Settings { get; set; } = "{}";
    public bool IsActive { get; set; } = true;

    // Navigation
    public Conversation Conversation { get; set; } = default!;
    public User User { get; set; } = default!;
}

public enum MemberRole { Member, Admin, Owner }