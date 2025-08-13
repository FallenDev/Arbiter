namespace Arbiter.Net;

public class NetworkPacketEventArgs(NetworkPacket packet, IReadOnlyCollection<byte> payload, byte? sequence = null)
    : EventArgs
{
    public NetworkPacket Packet { get; } = packet;
    public byte? Sequence { get; } = sequence;
    public IReadOnlyCollection<byte> Payload { get; } = payload;
}