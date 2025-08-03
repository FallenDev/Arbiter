namespace Arbiter.Net;

public class NetworkPacketEventArgs(NetworkPacket packet, IReadOnlyCollection<byte> payload)
    : EventArgs
{
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyCollection<byte> Payload { get; } = payload;
}