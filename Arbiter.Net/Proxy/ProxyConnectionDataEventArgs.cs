namespace Arbiter.Net.Proxy;

public class ProxyConnectionDataEventArgs(
    ProxyConnection connection,
    NetworkDirection direction,
    NetworkPacket encrypted,
    NetworkPacket decrypted)
    : ProxyConnectionEventArgs(connection)
{
    public NetworkDirection Direction { get; } = direction;
    public NetworkPacket Encrypted { get; } = encrypted;
    public NetworkPacket Decrypted { get; } = decrypted;
}