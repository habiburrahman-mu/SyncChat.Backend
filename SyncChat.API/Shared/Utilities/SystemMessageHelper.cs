using Newtonsoft.Json;
using SyncChat.API.Shared.Constants;

namespace SyncChat.API.Shared.Utilities;

public static class SystemMessageHelper
{
    public static string ConversationCreated(long createdBy)
    {
        return JsonConvert.SerializeObject(new
        {
            Type = SystemMessageType.ConversationCreated,
            CreatedBy = createdBy
        });
    }

    public static string MemberAdded(long userId, long addedBy)
    {
        return JsonConvert.SerializeObject(new
        {
            Type = SystemMessageType.MemberAdded,
            UserId = userId,
            AddedBy = addedBy
        });
    }
}
