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

    private static string Serialize(object message) => JsonConvert.SerializeObject(message);
}
