using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.Net.Proxy;

public partial class ProxyConnection
{
    private readonly NetworkPacketFilterCollection _clientFilters = new();
    private readonly NetworkPacketFilterCollection _serverFilters = new();

    public NetworkFilterRef AddFilter<T>(ClientMessageFilter<T> filter) where T : IClientMessage
    {
        CheckIfDisposed();
        
        var messageType = typeof(T);
        var command = _clientMessageFactory.GetMessageCommand(messageType);

        if (command is null)
        {
            throw new ArgumentException($"Message type {messageType.FullName} is not a registered client message type.",
                nameof(filter));
        }

        return AddFilter(command.Value, filter);
    }

    public NetworkFilterRef AddFilter<T>(ServerMessageFilter<T> filter) where T : IServerMessage
    {
        CheckIfDisposed();
        
        var messageType = typeof(T);
        var command = _serverMessageFactory.GetMessageCommand(messageType);

        if (command is null)
        {
            throw new ArgumentException($"Message type {messageType.FullName} is not a registered server message type.",
                nameof(filter));
        }

        return AddFilter(command.Value, filter);
    }

    public NetworkFilterRef AddFilter(ClientCommand command, INetworkPacketFilter filter)
    {
        CheckIfDisposed();
        return _clientFilters.AddFilter((byte)command, filter);
    }

    public NetworkFilterRef AddFilter(ServerCommand command, INetworkPacketFilter filter)
    {
        CheckIfDisposed();
        return _serverFilters.AddFilter((byte)command, filter);
    }

    public bool RemoveFilter(ClientCommand command, string name)
    {
        CheckIfDisposed();
        return _clientFilters.RemoveFilter((byte)command, name);
    }

    public bool RemoveFilter(ServerCommand command, string name)
    {
        CheckIfDisposed();
        return _serverFilters.RemoveFilter((byte)command, name);
    }
    
    public NetworkFilterRef AddGlobalFilter(ProxyDirection direction, INetworkPacketFilter filter)
    {
        CheckIfDisposed();
        
        var filters = direction switch
        {
            ProxyDirection.ClientToServer => _clientFilters,
            ProxyDirection.ServerToClient => _serverFilters,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        return filters.AddGlobalFilter(filter);
    }

    public bool RemoveGlobalFilter(ProxyDirection direction, string name)
    {
        CheckIfDisposed();
        
        var filters = direction switch
        {
            ProxyDirection.ClientToServer => _clientFilters,
            ProxyDirection.ServerToClient => _serverFilters,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        return filters.RemoveGlobalFilter(name);
    }

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
            foreach (var filter in filters.GetFilters(command).Where(f => f.IsEnabled))
            {
                // Add the filter name for back-tracing
                if (filter.Name is not null)
                {
                    result.AddFilterName(filter.Name);
                }

                var param = filter.Parameter;
                output = filter.Handler(this, output, param);

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

            // If the input does not match the output byte-for-byte, then mark as replaced
            if (result.Output is not null && !ReferenceEquals(result.Input, result.Output) &&
                (result.Input.Count() != result.Output.Count() || !result.Input.SequenceEqual(result.Output)))
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