
namespace Arbiter.Net.Filters;

public class NetworkPacketFilter : INetworkPacketFilter
{
    public string? Name { get; set; }
    public int Priority { get; set; } = 10;
    public object? Parameter { get; }
    public NetworkFilterHandler Handler { get; }

    public NetworkPacketFilter(NetworkFilterHandler handler, object? parameter = null)
    {
        Handler = handler;
        Parameter = parameter;
    }
}