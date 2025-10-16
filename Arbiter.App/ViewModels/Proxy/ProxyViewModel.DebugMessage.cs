using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private readonly Dictionary<string, Regex> _regexCache = [];

    private NetworkFilterRef? _emptyWorldMessageFilter;
    private NetworkFilterRef? _worldMessageFilter;

    private void AddDebugMessageFilters(DebugSettings settings, IReadOnlyList<MessageFilter> filters)
    {
        _emptyWorldMessageFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerWorldMessageMessage>(HandleEmptyWorldMessageMessage, settings)
            {
                Name = "Debug_WorldMessageFilter",
                Priority = DebugFilterPriority
            });

        _worldMessageFilter = _proxyServer.AddFilter(
            new ServerMessageFilter<ServerWorldMessageMessage>(HandleWorldMessage, filters.ToList())
            {
                Name = "Debug_EmptyWorldMessageFilter",
                Priority = DebugFilterPriority - 10
            });
    }

    private void RemoveDebugMessageFilters()
    {
        _emptyWorldMessageFilter?.Unregister();
        _worldMessageFilter?.Unregister();

        _regexCache.Clear();
    }

    private static NetworkPacket? HandleEmptyWorldMessageMessage(ProxyConnection connection,
        ServerWorldMessageMessage message,
        object? parameter, NetworkMessageFilterResult<ServerWorldMessageMessage> result)
    {
        if (parameter is not DebugSettings filterSettings || filterSettings is { IgnoreEmptyMessages: false })
        {
            return result.Passthrough();
        }

        // Block the packet if the message is empty
        return string.IsNullOrWhiteSpace(message.Message.Trim()) ? result.Block() : result.Passthrough();
    }

    private NetworkPacket? HandleWorldMessage(ProxyConnection connection, ServerWorldMessageMessage message,
        object? parameter, NetworkMessageFilterResult<ServerWorldMessageMessage> result)
    {
        if (parameter is not IReadOnlyList<MessageFilter> filters)
        {
            return result.Passthrough();
        }

        // Pass through non-bar or world messages
        if (message.MessageType != WorldMessageType.BarMessage && message.MessageType != WorldMessageType.WorldShout)
        {
            return result.Passthrough();
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
                return result.Block();
            }
        }

        return result.Passthrough();
    }
}