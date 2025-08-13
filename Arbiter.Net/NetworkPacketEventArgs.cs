namespace Arbiter.Net;

public class NetworkPacketEventArgs(NetworkAction action, NetworkPacket packet, NetworkPacket encryptedPacket)
    : EventArgs
{
    public NetworkAction Action { get; } = action;
    public NetworkPacket Packet { get; } = packet;
    public NetworkPacket EncryptedPacket { get; } = encryptedPacket;
    public bool IsEncrypted => !ReferenceEquals(Packet, EncryptedPacket);
}