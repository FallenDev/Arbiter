namespace Arbiter.Net;

public class NetworkTransferEventArgs(NetworkDirection direction, NetworkPacket encrypted, NetworkPacket decrypted)
    : EventArgs
{
    public NetworkDirection Direction { get; } = direction;
    public NetworkPacket Encrypted { get; } = encrypted;
    public NetworkPacket Decrypted { get; } = decrypted;
}