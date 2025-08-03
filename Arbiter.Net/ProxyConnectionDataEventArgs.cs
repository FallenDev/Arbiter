namespace Arbiter.Net;

public class ProxyConnectionDataEventArgs(
    ProxyConnection connection,
    NetworkPacket packet,
    IReadOnlyCollection<byte> payload)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyCollection<byte> Payload { get; } = payload;
}