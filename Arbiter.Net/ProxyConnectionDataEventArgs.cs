using Arbiter.Net;

namespace Arbiter.Net;

public class ProxyConnectionDataEventArgs(ProxyConnection connection, NetworkPacket packet, ProxyDirection direction)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkPacket Packet { get; } = packet;
    public ProxyDirection Direction { get; } = direction;
}