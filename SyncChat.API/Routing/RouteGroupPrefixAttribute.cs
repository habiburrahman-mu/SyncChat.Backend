namespace SyncChat.API.Routing;

[AttributeUsage(AttributeTargets.Interface)]
public class RouteGroupPrefixAttribute : Attribute
{
    public string Prefix { get; }
    public string? GroupName { get; }

    public RouteGroupPrefixAttribute(string prefix, string? groupName = null)
    {
        Prefix = prefix;
        GroupName = groupName;
    }

}
