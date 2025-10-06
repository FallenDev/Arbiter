using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private const string DebugWorldMessageFilterName = "Debug_WorldMessageFilter";
    private const string DebugEmptyWorldMessageFilterName = "Debug_EmptyWorldMessageFilter";

    private readonly Dictionary<string, Regex> _regexCache = new();
    
    private void AddDebugMessageFilters(DebugSettings settings, IReadOnlyList<MessageFilter> filters)
    {
        if (settings.IgnoreEmptyMessages)
        {
            _proxyServer.AddFilter(ServerCommand.WorldMessage,
                new NetworkPacketFilter(HandleEmptyWorldMessageMessage, settings)
                {
                    Name = DebugEmptyWorldMessageFilterName,
                    Priority = int.MaxValue
                });
        }
        
        if (filters.Count > 0)
        {
            _proxyServer.AddFilter(ServerCommand.WorldMessage,
                new NetworkPacketFilter(HandleWorldMessage, filters.ToList())
                {
                    Name = DebugWorldMessageFilterName,
                    Priority = int.MaxValue - 1
                });
        }
    }

    private void RemoveDebugMessageFilters()
    {
        _proxyServer.RemoveFilter(ServerCommand.WorldMessage, DebugEmptyWorldMessageFilterName);
        _proxyServer.RemoveFilter(ServerCommand.WorldMessage, DebugWorldMessageFilterName);
        
        _regexCache.Clear();
    }

    private NetworkPacket? HandleWorldMessage(ProxyConnection connection, NetworkPacket packet, object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not IReadOnlyList<MessageFilter> filters)
        {
            return packet;
        }

        // Only block messages that are bar messages or world shouts
        if (!_serverMessageFactory.TryCreate<ServerWorldMessageMessage>(serverPacket, out var message) ||
            message.MessageType != WorldMessageType.BarMessage && message.MessageType != WorldMessageType.WorldShout)
        {
            return packet;
        }

        foreach (var filter in filters)
        {
            if (!_regexCache.TryGetValue(filter.Pattern, out var regex))
            {
                regex = new Regex(filter.Pattern, RegexOptions.IgnoreCase);
                _regexCache[filter.Pattern] = regex;
            }

            if (filter.IsEnabled && regex.IsMatch(message.Message))
            {
                return null;
            }
        }

        return packet;
    }

    private NetworkPacket? HandleEmptyWorldMessageMessage(ProxyConnection connection, NetworkPacket packet,
        object? parameter)
    {
        // Ensure the packet is the correct type and we have settings as a parameter
        if (packet is not ServerPacket serverPacket || parameter is not DebugSettings filterSettings)
        {
            return packet;
        }

        if (filterSettings is { IgnoreEmptyMessages: false } ||
            !_serverMessageFactory.TryCreate<ServerWorldMessageMessage>(serverPacket, out var message))
        {
            return packet;
        }

        // Block the packet if the message is empty
        return !string.IsNullOrWhiteSpace(message.Message.Trim()) ? packet : null;
    }
}