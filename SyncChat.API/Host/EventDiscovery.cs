using SyncChat.API.Shared.Events;
using System.Reflection;

namespace SyncChat.API.Host;

public static class EventDiscovery
{
    public static IServiceCollection RegisterDomainEventHandlers(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var handlerTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(i => new { Service = i, Implementation = t }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Service, handler.Implementation);
        }

        return services;
    }
}
