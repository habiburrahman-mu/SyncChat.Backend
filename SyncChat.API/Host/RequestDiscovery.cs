using FluentValidation;
using SyncChat.API.Shared.Sender.Contracts;
using System.Reflection;

namespace SyncChat.API.Host;

public static class RequestDiscovery
{
    public static void RegisterRequestHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly();

        var genericRequestHandlerType = typeof(IQueryHandler<,>);
        var genericVoidRequestHandlerType = typeof(IQueryHandler<>);

        var genericCommandHandlerType = typeof(ICommandHandler<,>);
        var genericVoidCommandHandlerType = typeof(ICommandHandler<>);

        var handlerTypes = assembly.
            GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType
                        && (i.GetGenericTypeDefinition() == genericRequestHandlerType 
                            || i.GetGenericTypeDefinition() == genericVoidRequestHandlerType
                            || i.GetGenericTypeDefinition() == genericCommandHandlerType 
                            || i.GetGenericTypeDefinition() == genericVoidCommandHandlerType))
                .Select(i => new { Interface = i, Implementation = type }));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType.Interface, handlerType.Implementation);
        }

        // Register FluentValidation Validators
        var validatorTypes = assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new { Interface = i, Implementation = type }));

        foreach (var validatorType in validatorTypes)
        {
            services.AddTransient(validatorType.Interface, validatorType.Implementation);
        }
    }
}
