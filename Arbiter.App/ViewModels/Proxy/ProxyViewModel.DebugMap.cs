using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugMapInfoFilterName = "Debug_MapInfoFilter";

    private void AddDebugMapFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.MapInfo,
            new NetworkPacketFilter(HandleMapInfoMessagePacket, settings)
            {
                Name = DebugMapInfoFilterName,
                Priority = int.MaxValue
            });
    }

    private void RemoveDebugMapFilters() => _proxyServer.RemoveFilter(ServerCommand.MapInfo, DebugMapInfoFilterName);

    private NetworkPacket HandleMapInfoMessagePacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { EnableTabMap: false, DisableWeatherEffects: false } ||
            !_serverMessageFactory.TryCreate<ServerMapInfoMessage>(serverPacket, out var message))
        {
            return packet;
        }

        if (filterSettings.EnableTabMap)
        {
            message.Flags &= ~MapFlags.NoMap;
        }

        if (filterSettings.DisableWeatherEffects)
        {
            message.Flags &= ~(MapFlags.Rain | MapFlags.Snow | MapFlags.Winter);
        }

        // Build a new packet with the modified map info data
        var builder = new NetworkPacketBuilder(ServerCommand.MapInfo);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}