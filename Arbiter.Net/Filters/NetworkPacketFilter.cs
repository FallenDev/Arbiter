using Arbiter.Net.Proxy;

namespace Arbiter.Net.Filters;

public class NetworkPacketFilter : INetworkPacketFilter
{
    public string? Name { get; set; }
    public int Priority { get; set; } = 10;
    public object? Parameter { get; }
    public NetworkFilterHandler Handler { get; }

    public NetworkPacketFilter(NetworkFilterHandler handler)
        : this((connection, packet, _) => handler(connection, packet), null)
    {
    }

    public NetworkPacketFilter(NetworkFilterHandler handler, object? parameter)
    {
        Handler = handler;
        Parameter = parameter;
    }
}