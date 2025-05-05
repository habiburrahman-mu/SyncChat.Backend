namespace SyncChat.API.Routing;

[AttributeUsage(AttributeTargets.Interface)]
public class RouteGroupPrefixAttribute : Attribute
{
    public string Prefix { get; }

    public RouteGroupPrefixAttribute(string prefix)
    {
        Prefix = prefix;
    }

}
