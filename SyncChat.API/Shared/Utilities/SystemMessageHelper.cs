using Newtonsoft.Json;
using SyncChat.API.Shared.Constants;

namespace SyncChat.API.Shared.Utilities;

public static class SystemMessageHelper
{
    public static string ConversationCreated(long createdBy)
    {
        return Serialize(new
        {
            Type = SystemMessageType.ConversationCreated,
            CreatedBy = createdBy
        });
    }

    public static string MemberAdded(long userId, long addedBy)
    {
        return Serialize(new
        {
            Type = SystemMessageType.MemberAdded,
            UserId = userId,
            AddedBy = addedBy
        });
    }

    public static string MemberLeft(long userId)
    {
        return Serialize(new
        {
            Type = SystemMessageType.MemberLeft,
            UserId = userId,
        });
    }

    public static string MemberRemoved(long userId, long removedBy)
    {
        return Serialize(new
        {
            Type = SystemMessageType.MemberRemoved,
            UserId = userId,
            RemovedBy = removedBy
        });
    }

    public static string MemberPromotedToAdmin(long userId, long promotedBy)
    {
        return Serialize(new
        {
            Type = SystemMessageType.MemberPromotedToAdmin,
            UserId = userId,
            PromotedBy = promotedBy
        });
    }

    private static string Serialize(object message) => JsonConvert.SerializeObject(message);
}
