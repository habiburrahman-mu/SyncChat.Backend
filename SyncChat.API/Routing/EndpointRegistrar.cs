using System.Reflection;

namespace SyncChat.API.Routing;

public static class EndpointRegistrar
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        List<Type> endpointInterfaces = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsInterface &&
                        typeof(IEndpoint).IsAssignableFrom(t) &&
                        t.GetCustomAttribute<RouteGroupPrefixAttribute>() is not null)
            .ToList();

        RouteGroupBuilder api = app.MapGroup("/api");

        foreach (Type endpointInterface in endpointInterfaces)
        {
            RouteGroupPrefixAttribute prefixAttribute = endpointInterface.GetCustomAttribute<RouteGroupPrefixAttribute>()!;

            string routeGroupPrefix = prefixAttribute.Prefix;
            RouteGroupBuilder group = api.MapGroup(routeGroupPrefix);

            if(prefixAttribute.GroupName is not null)
            {
                group.WithTags(prefixAttribute.GroupName);
            }

            if(prefixAttribute.HasAuthorization)
            {
                group.RequireAuthorization();
            }

            List<Type> implementations = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => endpointInterface.IsAssignableFrom(t) &&
                            !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (Type implementation in implementations)
            {
                IEndpoint instance = (IEndpoint)Activator.CreateInstance(implementation)!;
                instance.Map(group);
            }
        }
    }
}
