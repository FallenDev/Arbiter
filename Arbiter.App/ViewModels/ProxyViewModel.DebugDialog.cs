using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels;

public partial class ProxyViewModel
{
    private const string DebugShowDialogFilterName = "Debug_ShowDialogFilter";
    private const string DebugShowDialogMenuFilterName = "Debug_ShowDialogMenuFilter";
    
    private void AddDebugDialogFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.ShowDialog, new NetworkPacketFilter(HandleDialogPacket, settings)
        {
            Name = DebugShowDialogFilterName,
            Priority = int.MaxValue
        });

        _proxyServer.AddFilter(ServerCommand.ShowDialogMenu, new NetworkPacketFilter(HandleDialogMenuPacket, settings)
        {
            Name = DebugShowDialogMenuFilterName,
            Priority = int.MaxValue
        });
    }

    private void RemoveDebugDialogFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.ShowDialog, DebugShowDialogFilterName);
        _proxyServer.RemoveFilter(ServerCommand.ShowDialogMenu, DebugShowDialogMenuFilterName);
    }

    private NetworkPacket HandleDialogPacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        // If no debug settings enabled, ignore the packet
        if (filterSettings is { ShowDialogId: false })
        {
            return packet;
        }

        // Ignore if the packet could not be read as the expected message type
        if (!_serverMessageFactory.TryCreate<ServerShowDialogMessage>(serverPacket, out var message))
        {
            return packet;
        }

        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        message.Name = $"{name} [0x{message.EntityId:X4}]";
        
        // Build a new packet with the modified dialog data
        var builder = new NetworkPacketBuilder(ServerCommand.ShowDialog);
        message.Serialize(builder);

        return builder.ToPacket();
    }

    private NetworkPacket HandleDialogMenuPacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { ShowDialogId: false } ||
            !_serverMessageFactory.TryCreate<ServerShowDialogMenuMessage>(serverPacket, out var message))
        {
            return packet;
        }

        var name = !string.IsNullOrWhiteSpace(message.Name) ? message.Name : message.EntityType.ToString();
        message.Name = $"{name} [0x{message.EntityId:X4}]";

        // Build a new packet with the modified dialog data
        var builder = new NetworkPacketBuilder(ServerCommand.ShowDialogMenu);
        message.Serialize(builder);

        return builder.ToPacket();
    }
}