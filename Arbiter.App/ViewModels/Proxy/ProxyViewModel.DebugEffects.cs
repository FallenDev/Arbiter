using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugUpdateStatsFilterName = "Debug_UpdateStatsFilter";
    
    private void AddDebugEffectsFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.UpdateStats,
            new NetworkPacketFilter(HandleUpdateStatsPacket, settings)
            {
                Name = DebugUpdateStatsFilterName,
                Priority = int.MaxValue
            });
    }

    private void RemoveDebugEffectsFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.UpdateStats, DebugUpdateStatsFilterName);
    }

    private NetworkPacket HandleUpdateStatsPacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { DisableBlind: false } ||
            !_serverMessageFactory.TryCreate<ServerUpdateStatsMessage>(serverPacket, out var message) ||
            message.IsBlinded is not true)
        {
            return packet;
        }

        message.IsBlinded = false;

        // Build a new packet with the modified status data
        var builder = new NetworkPacketBuilder(ServerCommand.UpdateStats);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}