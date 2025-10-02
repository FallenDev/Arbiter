using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugEmptyWorldMessageFilterName = "Debug_EmptyWorldMessageFilter";

    private void AddDebugMessageFilters(DebugSettings settings)
    {
        _proxyServer.AddFilter(ServerCommand.WorldMessage,
            new NetworkPacketFilter(HandleEmptyWorldMessagePacket, settings)
            {
                Name = DebugEmptyWorldMessageFilterName,
                Priority = int.MaxValue
            });
    }

    private void RemoveDebugMessageFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.WorldMessage, DebugEmptyWorldMessageFilterName);
    }

    private NetworkPacket? HandleEmptyWorldMessagePacket(NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { IgnoreEmptyMessages: true } ||
            !_serverMessageFactory.TryCreate<ServerWorldMessageMessage>(serverPacket, out var message))
        {
            return packet;
        }

        // Block the packet if the message is empty
        return !string.IsNullOrWhiteSpace(message.Message.Trim()) ? packet : null;
    }
}