namespace Arbiter.Net;

public class ProxyConnectionDataEventArgs(
    ProxyConnection connection,
    NetworkAction action,
    NetworkPacket packet,
    NetworkPacket encryptedPacket)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkAction Action { get; } = action;
    public NetworkPacket Packet { get; } = packet;
    public NetworkPacket EncryptedPacket { get; } = encryptedPacket;
    public bool IsEncrypted => !ReferenceEquals(Packet, EncryptedPacket);
}