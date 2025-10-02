namespace Arbiter.Net.Filters;

public class NetworkPacketFilter : INetworkPacketFilter
{
    public string? Name { get; set; }
    public int Priority { get; set; } = 10;
    public object? Parameter { get; }
    public Func<NetworkPacket, object?, NetworkPacket?> Filter { get; }

    public NetworkPacketFilter(Func<NetworkPacket, NetworkPacket?> filter)
        : this((packet, _) => filter(packet), null)
    {
    }

    public NetworkPacketFilter(Func<NetworkPacket, object?, NetworkPacket?> filter, object? parameter)
    {
        Filter = filter;
        Parameter = parameter;
    }
}