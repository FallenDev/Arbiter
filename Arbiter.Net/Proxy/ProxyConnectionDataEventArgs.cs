namespace Arbiter.Net.Proxy;

public class ProxyConnectionDataEventArgs(
    ProxyConnection connection,
    NetworkAction action,
    NetworkPacket packet,
    IReadOnlyList<byte> rawData)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkAction Action { get; } = action;
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyList<byte> RawData { get; } = rawData;
}