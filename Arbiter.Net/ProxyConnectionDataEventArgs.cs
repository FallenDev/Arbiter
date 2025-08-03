namespace Arbiter.Net;

public class ProxyConnectionDataEventArgs(ProxyConnection connection, NetworkPacket packet)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkPacket Packet { get; } = packet;
}