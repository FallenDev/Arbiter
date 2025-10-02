using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels;

public partial class ProxyViewModel
{
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

        var name = $"{message.Name ?? "Entity"} (0x{message.EntityId:x4})";
        message.Name = name;

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

        // If no debug settings enabled, ignore the packet
        if (filterSettings is { ShowDialogId: false })
        {
            return packet;
        }

        // Ignore if the packet could not be read as the expected message type
        if (!_serverMessageFactory.TryCreate<ServerShowDialogMenuMessage>(serverPacket, out var message))
        {
            return packet;
        }

        var name = $"{message.Name ?? "Entity"} 0x{message.EntityId:x4}";
        message.Name = name;

        // Build a new packet with the modified dialog data
        var builder = new NetworkPacketBuilder(ServerCommand.ShowDialogMenu);
        message.Serialize(builder);
        
        return builder.ToPacket();
    }
}