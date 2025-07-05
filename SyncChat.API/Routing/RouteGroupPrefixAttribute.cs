namespace SyncChat.API.Routing;

[AttributeUsage(AttributeTargets.Interface)]
public class RouteGroupPrefixAttribute : Attribute
{
    public string Prefix { get; }
    public string? GroupName { get; }
    public bool HasAuthorization { get; set; }

    public RouteGroupPrefixAttribute(string prefix, string? groupName = null, bool hasAuthorization = false)
    {
        Prefix = prefix;
        GroupName = groupName;
        HasAuthorization = hasAuthorization;
    }

}
