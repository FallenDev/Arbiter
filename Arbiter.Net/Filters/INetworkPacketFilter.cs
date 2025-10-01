namespace Arbiter.Net.Filters;

public interface INetworkPacketFilter
{
    string Name { get; set; }
    int Priority { get; set; }
    object? Parameter { get; }
    Func<NetworkPacket, object?, NetworkPacket?> Filter { get; }
}