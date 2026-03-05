using SyncChat.API.Shared.Events;
using System.Threading.Channels;

namespace SyncChat.API.Infrastructure.Outbox;

public readonly record struct DomainEventEnvelope(Guid OutboxMessageId, IDomainEvent DomainEvent);

public sealed class DomainEventChannel
{
    private readonly Channel<DomainEventEnvelope> channel = Channel.CreateUnbounded<DomainEventEnvelope>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelWriter<DomainEventEnvelope> Writer => channel.Writer;
    public ChannelReader<DomainEventEnvelope> Reader => channel.Reader;
}
