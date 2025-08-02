namespace Arbiter.Net;

public class NetworkPacketEventArgs(QueuedNetworkPacket packet) : EventArgs
{
    public QueuedNetworkPacket Packet { get; } = packet;
}