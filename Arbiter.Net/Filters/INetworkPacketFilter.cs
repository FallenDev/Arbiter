using Arbiter.Net.Proxy;

namespace Arbiter.Net.Filters;

public delegate NetworkPacket? NetworkFilterHandler(ProxyConnection connection, NetworkPacket packet,
    object? parameter = null);

public interface INetworkPacketFilter
{
    string? Name { get; set; }
    int Priority { get; set; }
    object? Parameter { get; }
    NetworkFilterHandler Handler { get; }
}