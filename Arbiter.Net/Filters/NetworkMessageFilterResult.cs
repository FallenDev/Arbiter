namespace Arbiter.Net.Filters;

public class NetworkMessageFilterResult<TMessage>
{
    private readonly Func<NetworkPacket> _originalPacketFactory;
    private readonly Func<TMessage, NetworkPacket> _modifiedPacketFactory;

    internal NetworkMessageFilterResult(Func<NetworkPacket> originalPacketFactory,
        Func<TMessage, NetworkPacket> modifiedPacketFactory)
    {
        _originalPacketFactory = originalPacketFactory;
        _modifiedPacketFactory = modifiedPacketFactory;
    }

    public NetworkPacket? Block() => null;

    public NetworkPacket Passthrough() => _originalPacketFactory();

    public NetworkPacket Replace(TMessage message) => _modifiedPacketFactory(message);
}