namespace Arbiter.Net;

public class QueuedNetworkPacket(NetworkPacket packet, ProxyDirection direction)
{
    public NetworkPacket Packet { get; } = packet;
    public ProxyDirection Direction { get; } = direction;
}