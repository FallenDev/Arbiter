using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugUserIdFilterName = "Debug_UserIdFilter";
    private const string DebugMapInfoFilterName = "Debug_MapInfoFilter";

    private void AddDebugMapFilters(DebugSettings settings)
    {
        if (settings.EnableZoomedOutMap)
        {
            _proxyServer.AddFilter(ServerCommand.UserId,
                new NetworkPacketFilter(HandleUserIdMessage, settings)
                {
                    Name = DebugUserIdFilterName,
                    Priority = int.MaxValue
                });
        }

        if (settings.EnableTabMap || settings.DisableWeatherEffects || settings.DisableDarkness)
        {
            _proxyServer.AddFilter(ServerCommand.MapInfo,
                new NetworkPacketFilter(HandleMapInfoMessage, settings)
                {
                    Name = DebugMapInfoFilterName,
                    Priority = int.MaxValue
                });
        }
    }

    private void RemoveDebugMapFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.UserId, DebugUserIdFilterName);
        _proxyServer.RemoveFilter(ServerCommand.MapInfo, DebugMapInfoFilterName);
    }

    private NetworkPacket HandleUserIdMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { EnableZoomedOutMap: false } ||
            !_serverMessageFactory.TryCreate<ServerUserIdMessage>(serverPacket, out var message) ||
            !filterSettings.EnableZoomedOutMap)
        {
            return packet;
        }

        message.Class = CharacterClass.Rogue;

        // Build a new packet with the modified user data
        var builder = new NetworkPacketBuilder(ServerCommand.UserId);
        message.Serialize(builder);

        return builder.ToPacket();
    }

    private NetworkPacket HandleMapInfoMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { EnableTabMap: false, DisableWeatherEffects: false, DisableDarkness: false } ||
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

        if (filterSettings.DisableDarkness)
        {
            message.Flags &= ~MapFlags.Darkness;
        }

        // Build a new packet with the modified map info data
        var builder = new NetworkPacketBuilder(ServerCommand.MapInfo);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}