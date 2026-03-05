using SyncChat.API.Shared.Events;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class ImmediateEventDispatcher : BackgroundService
{
    private readonly DomainEventChannel domainEventChannel;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ImmediateEventDispatcher> logger;

    public ImmediateEventDispatcher(
        DomainEventChannel domainEventChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ImmediateEventDispatcher> logger)
    {
        this.domainEventChannel = domainEventChannel;
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IDomainEvent domainEvent in domainEventChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                IServiceProvider provider = scope.ServiceProvider;

                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = provider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    await ((dynamic)handler!).HandleAsync((dynamic)domainEvent, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Immediate dispatch failed for {EventType}. Outbox will retry.", domainEvent.GetType().Name);
            }
        }
    }
}
