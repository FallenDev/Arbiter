using Arbiter.Net.Client;
using Arbiter.Net.Filters;
using Arbiter.Net.Server;

namespace Arbiter.Net.Proxy;

public partial class ProxyServer
{
    private readonly ReaderWriterLockSlim _filtersLock = new();
    private readonly NetworkPacketFilterCollection _clientFilters = new();
    private readonly NetworkPacketFilterCollection _serverFilters = new();

    public void AddFilter(ClientCommand command, INetworkPacketFilter filter) =>
        AddFilterInternal(ProxyDirection.ClientToServer, (byte)command, filter);

    public void AddFilter(ServerCommand command, INetworkPacketFilter filter) =>
        AddFilterInternal(ProxyDirection.ServerToClient, (byte)command, filter);

    public bool RemoveFilter(ClientCommand command, string name) =>
        RemoveFilterInternal(ProxyDirection.ClientToServer, (byte)command, name);

    public bool RemoveFilter(ServerCommand command, string name) =>
        RemoveFilterInternal(ProxyDirection.ServerToClient, (byte)command, name);

    public void AddGlobalFilter(ProxyDirection direction, INetworkPacketFilter filter)
        => AddFilterInternal(direction, null, filter);

    public bool RemoveGlobalFilter(ProxyDirection direction, string name) =>
        RemoveFilterInternal(direction, null, name);
    
    private void AddFilterInternal(ProxyDirection direction, byte? command, INetworkPacketFilter filter)
    {
        var filters = direction switch
        {
            ProxyDirection.ClientToServer => _clientFilters,
            ProxyDirection.ServerToClient => _serverFilters,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        _filtersLock.EnterWriteLock();
        try
        {
            if (command.HasValue)
            {
                filters.AddFilter(command.Value, filter);
            }
            else
            {
                filters.AddGlobalFilter(filter);
            }
        }
        finally
        {
            _filtersLock.ExitWriteLock();
        }

        using var _ = _connectionsLock.EnterScope();

        foreach (var connection in _connections)
        {
            if (direction == ProxyDirection.ClientToServer)
            {
                // Add to the client filter collection
                if (command.HasValue)
                {
                    connection.AddFilter((ClientCommand)command.Value, filter);
                }
                else
                {
                    connection.AddGlobalFilter(ProxyDirection.ClientToServer, filter);
                }
            }
            else
            {
                // Add to the server filter collection
                if (command.HasValue)
                {
                    connection.AddFilter((ServerCommand)command.Value, filter);
                }
                else
                {
                    connection.AddGlobalFilter(ProxyDirection.ServerToClient, filter);
                }
            }
        }
    }

    private bool RemoveFilterInternal(ProxyDirection direction, byte? command, string name)
    {
        var filters = direction switch
        {
            ProxyDirection.ClientToServer => _clientFilters,
            ProxyDirection.ServerToClient => _serverFilters,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        _filtersLock.EnterWriteLock();
        try
        {
            if (command.HasValue)
            {
                filters.RemoveFilter(command.Value, name);
            }
            else
            {
                filters.RemoveGlobalFilter(name);
            }
        }
        finally
        {
            _filtersLock.ExitWriteLock();
        }

        var wasRemoved = false;

        using var _ = _connectionsLock.EnterScope();

        if (direction == ProxyDirection.ClientToServer)
        {
            // Remove from the client filter collection
            wasRemoved = _connections.Aggregate(wasRemoved, (current, connection) => (command.HasValue
                ? connection.RemoveFilter((ClientCommand)command.Value, name)
                : connection.RemoveGlobalFilter(ProxyDirection.ClientToServer, name)) || current);
        }
        else
        {
            // Remove from the server filter collection
            wasRemoved = _connections.Aggregate(wasRemoved, (current, connection) => (command.HasValue
                ? connection.RemoveFilter((ServerCommand)command.Value, name)
                : connection.RemoveGlobalFilter(ProxyDirection.ServerToClient, name)) || current);
        }

        return wasRemoved;
    }

    private void InheritFilters(ProxyConnection connection)
    {
        _filtersLock.EnterReadLock();

        try
        {
            for (var i = 0; i < byte.MaxValue; i++)
            {
                var command = (byte)i;
                var clientFilters = _clientFilters.GetFilters(command);
                var serverFilters = _serverFilters.GetFilters(command);

                foreach (var filter in clientFilters)
                {
                    connection.AddFilter((ClientCommand)command, filter);
                }

                foreach (var filter in serverFilters)
                {
                    connection.AddFilter((ServerCommand)command, filter);
                }
            }
        }
        finally
        {
            _filtersLock.ExitReadLock();
        }
    }
}