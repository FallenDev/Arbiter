namespace Arbiter.Net;

public class NetworkPacketEventArgs(NetworkPacket packet, ProxyDirection direction)
    : EventArgs
{
    public NetworkPacket Packet { get; } = packet;
    public ProxyDirection Direction { get; } = direction;
}