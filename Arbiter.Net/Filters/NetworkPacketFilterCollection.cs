using System.Collections.Immutable;

namespace Arbiter.Net.Filters;

public class NetworkPacketFilterCollection
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ImmutableArray<INetworkPacketFilter>[] _filters = new ImmutableArray<INetworkPacketFilter>[byte.MaxValue];

    public NetworkPacketFilterCollection()
    {
        InitializeFilters();
    }

    private void InitializeFilters()
    {
        for (var i = 0; i < _filters.Length; i++)
        {
            _filters[i] = ImmutableArray<INetworkPacketFilter>.Empty;
        }
    }

    public void AddFilter(byte command, INetworkPacketFilter filter)
    {
        _lock.EnterWriteLock();
        try
        {
            // Add the filter to the list, keep sorted by priority (highest first)
            var filters = _filters[command].Add(filter);
            _filters[command] = filters.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<INetworkPacketFilter> GetFilters(byte command)
    {
        _lock.EnterReadLock();
        try
        {
            return _filters[command];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void AddGlobalFilter(INetworkPacketFilter filter)
    {
        _lock.EnterWriteLock();
        try
        {
            // Global filters are just applied to all commands
            for (var i = 0; i < _filters.Length; i++)
            {
                // Add the filter to the list, keep sorted by priority (highest first)
                var filters = _filters[i].Add(filter);
                _filters[i] = filters.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool RemoveFilter(byte command, string name)
    {
        _lock.EnterWriteLock();
        try
        {
            var filter = _filters[command].FirstOrDefault(f => f.Name is not null && f.Name == name);
            if (filter is null)
            {
                return false;
            }

            _filters[command] = _filters[command].Remove(filter);
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool RemoveGlobalFilter(string name)
    {
        _lock.EnterWriteLock();
        try
        {
            var wasRemoved = false;

            // Global filters are just applied to all commands
            for (var i = 0; i < _filters.Length; i++)
            {
                var filter = _filters[i].FirstOrDefault(f => f.Name is not null && f.Name == name);
                if (filter is null)
                {
                    continue;
                }

                _filters[i] = _filters[i].Remove(filter);
                wasRemoved = true;
            }

            return wasRemoved;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            InitializeFilters();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}