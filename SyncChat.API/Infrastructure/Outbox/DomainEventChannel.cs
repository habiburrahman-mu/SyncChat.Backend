using SyncChat.API.Shared.Events;
using System.Threading.Channels;

namespace SyncChat.API.Infrastructure.Outbox;

public sealed class DomainEventChannel
{
    private readonly Channel<IDomainEvent> channel = Channel.CreateUnbounded<IDomainEvent>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelWriter<IDomainEvent> Writer => channel.Writer;
    public ChannelReader<IDomainEvent> Reader => channel.Reader;
}
