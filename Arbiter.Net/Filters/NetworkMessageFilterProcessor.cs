using System.Collections.Concurrent;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Proxy;
using Arbiter.Net.Serialization;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.Net.Filters;

internal class NetworkMessageFilterProcessor
{
    private readonly ConcurrentDictionary<(Type MessageType, string? Name), NetworkMessageFilterWrapper> _filters = [];
    private readonly IClientMessageFactory _clientMessageFactory;
    private readonly IServerMessageFactory _serverMessageFactory;

    public NetworkMessageFilterProcessor()
        : this(new ClientMessageFactory(), new ServerMessageFactory())
    {
    }

    public NetworkMessageFilterProcessor(IClientMessageFactory clientMessageFactory, IServerMessageFactory serverMessageFactory)
    {
        _clientMessageFactory = clientMessageFactory;
        _serverMessageFactory = serverMessageFactory;
    }

    public void AddMessageFilter<T>(INetworkMessageFilter<T> filter) where T : class
    {
        var wrapper = new NetworkMessageFilterWrapper<T>(filter);
        var key = (typeof(T), filter.Name);
        _filters.AddOrUpdate(key, wrapper, (_, _) => wrapper);
    }
    
    public bool RemoveMessageFilter<T>(string name) where T : class
    {
        var key = (typeof(T), name);
        return _filters.TryRemove(key, out _);
    }
    
    public void ClearMessageFilters() => _filters.Clear();

    public NetworkPacket? ProcessPacket(ProxyConnection connection, NetworkPacket packet)
    {
        try
        {
            return packet switch
            {
                ClientPacket clientPacket => ProcessClientPacket(connection, clientPacket),
                ServerPacket serverPacket => ProcessServerPacket(connection, serverPacket),
                _ => packet
            };
        }
        catch
        {
            return packet;
        }
    }

    private NetworkPacket? ProcessClientPacket(ProxyConnection connection, ClientPacket packet)
    {
        var messageType = _clientMessageFactory.GetMessageType(packet.Command);
        if (messageType is null)
        {
            return packet;
        }

        // Get the filters for this message type
        var matchingFilters = GetFiltersForMessageType(messageType);
        if (matchingFilters.Count == 0)
        {
            return packet;
        }

        // Try to create the message to filter
        if (!_clientMessageFactory.TryCreate(packet, out var message))
        {
            return packet;
        }

        // Process the message through all filters
        var processedMessage = ProcessMessage(connection, message, matchingFilters);
        if (processedMessage is null)
        {
            // Packet was blocked
            return null;
        }

        if (ReferenceEquals(processedMessage, message))
        {
            // Message was not modified, return packet as-is
            return packet;
        }

        // Message was changed, need to serialize new packet
        var builder = new NetworkPacketBuilder(packet.Command);
        (processedMessage as IClientMessage)!.Serialize(builder);
        return builder.ToPacket();
    }

    private NetworkPacket? ProcessServerPacket(ProxyConnection connection, ServerPacket packet)
    {
        var messageType = _serverMessageFactory.GetMessageType(packet.Command);
        if (messageType is null)
        {
            return packet;
        }

        // Get the filters for this message type
        var matchingFilters = GetFiltersForMessageType(messageType);
        if (matchingFilters.Count == 0)
        {
            return packet;
        }

        // Try to create the message to filter
        if (!_serverMessageFactory.TryCreate(packet, out var message))
        {
            return packet;
        }

        // Process the message through all filters
        var processedMessage = ProcessMessage(connection, message, matchingFilters);
        if (processedMessage is null)
        {
            // Packet was blocked
            return null;
        }

        if (ReferenceEquals(processedMessage, message))
        {
            // Message was not modified, return packet as-is
            return packet;
        }

        // Message was changed, need to serialize new packet
        var builder = new NetworkPacketBuilder(packet.Command);
        (processedMessage as IServerMessage)!.Serialize(builder);
        return builder.ToPacket();
    }

    private List<NetworkMessageFilterWrapper> GetFiltersForMessageType(Type messageType)
    {
        var filters = new List<NetworkMessageFilterWrapper>();

        foreach (var (key, wrappedFilter) in _filters)
        {
            if (key.MessageType == messageType)
            {
                filters.Add(wrappedFilter);
            }
        }

        // Return filters sorted by priority descending
        filters.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return filters;
    }

    private static object? ProcessMessage(ProxyConnection connection, object message,
        List<NetworkMessageFilterWrapper> filters)
    {
        var currentMessage = message;

        foreach (var filter in filters)
        {
            if (currentMessage is null)
            {
                break;
            }

            currentMessage = filter.Invoke(connection, currentMessage);
        }

        return currentMessage;
    }
}