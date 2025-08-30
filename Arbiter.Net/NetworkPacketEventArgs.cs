namespace Arbiter.Net;

public class NetworkPacketEventArgs(NetworkAction action, NetworkPacket packet, IReadOnlyList<byte> rawData)
    : EventArgs
{
    public NetworkAction Action { get; } = action;
    public NetworkPacket Packet { get; } = packet;
    public IReadOnlyList<byte> RawData { get; } = rawData;
}