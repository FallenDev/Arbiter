using Arbiter.Net.Client;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;

namespace Arbiter.Net.Proxy;

public partial class ProxyConnection
{
    private readonly NetworkPacketFilterCollection _clientFilters = new();
    private readonly NetworkPacketFilterCollection _serverFilters = new();

    private NetworkFilterResult FilterPacket(NetworkPacket packet)
    {
        var command = packet.Command;
        var filters = packet switch
        {
            ClientPacket => _clientFilters,
            ServerPacket => _serverFilters,
            _ => throw new InvalidOperationException("Invalid packet type")
        };

        var result = new NetworkFilterResult
        {
            Input = packet
        };

        try
        {
            var output = packet;
            foreach (var filter in filters.GetFilters(command))
            {
                var param = filter.Parameter;
                output = filter.Filter(output, param);
                
                // Process the next filter using the output from the previous one
                if (output is not null)
                {
                    continue;
                }
                
                // If the filter returned null, then block the packet and do not continue the chain
                result.Action = NetworkFilterAction.Block;
                result.Output = null;
                return result;
            }
            result.Output = output;

            // If the input does not match the output, then mark as replaced
            if (result.Output is not null && !ReferenceEquals(result.Input, result.Output))
            {
                result.Action = NetworkFilterAction.Replace;
            }
        }
        catch (Exception ex)
        {
            // Filter threw an exception, so act as a simple passthrough
            result.Action = NetworkFilterAction.Allow;
            result.Output = packet;
            result.Exception = ex;
        }

        return result;
    }
}