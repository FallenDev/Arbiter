namespace Arbiter.Net;

public class ProxyConnectionDataEventArgs(
    ProxyConnection connection,
    NetworkPacket packet,
    IReadOnlyCollection<byte> payload,
    byte? sequence = null)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkPacket Packet { get; } = packet;
    public byte? Sequence = sequence;
    public IReadOnlyCollection<byte> Payload { get; } = payload;
}