using Arbiter.Net;

namespace Arbiter.App.ViewModels;

public class TracePacketViewModel(ProxyConnection connection, NetworkPacket packet, ProxyDirection direction)
    : ViewModelBase
{
    public ProxyConnection Connection { get; } = connection;
    public NetworkPacket Packet { get; } = packet;
    public ProxyDirection Direction { get; } = direction;

    public bool IsClient => Direction == ProxyDirection.ClientToServer;
    public bool IsServer => Direction == ProxyDirection.ServerToClient;
}