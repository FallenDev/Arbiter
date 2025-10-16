using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.Threading;
using Arbiter.Net.Proxy;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityListViewModel : ViewModelBase
{
    private readonly Debouncer _searchRefreshDebouncer = new(TimeSpan.FromMilliseconds(50), Dispatcher.UIThread);
    private readonly IEntityStore _entityStore;
    private readonly IPlayerService _playerService;
    private readonly ConcurrentObservableCollection<EntityViewModel> _allEntities = [];

    private long _indexCounter = 1;
    private uint? _searchEntityId;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private EntitySortOrder _sortOrder = EntitySortOrder.FirstSeen;
    [ObservableProperty] private bool _includePlayers = true;
    [ObservableProperty] private bool _includeNpcs = true;
    [ObservableProperty] private bool _includeMonsters;
    [ObservableProperty] private bool _includeItems;
    [ObservableProperty] private bool _includeReactors;

    public FilteredObservableCollection<EntityViewModel> FilteredEntities { get; }
    public ObservableCollection<EntityViewModel> SelectedEntities { get; } = [];

    public List<EntitySortOrder> AvailableSortOrders =>
        [EntitySortOrder.FirstSeen, EntitySortOrder.Name, EntitySortOrder.Id];
    
    public EntityListViewModel(ProxyServer proxyServer, IEntityStore entityStore, IPlayerService playerService)
    {
        _entityStore = entityStore;
        _playerService = playerService;

        _entityStore.EntityAdded += OnEntityAdded;
        _entityStore.EntityUpdated += OnEntityUpdated;
        _entityStore.EntityRemoved += OnEntityRemoved;

        FilteredEntities = new FilteredObservableCollection<EntityViewModel>(_allEntities, MatchesFilter);

        AddPacketFilters(proxyServer);
    }

    private bool MatchesFilter(EntityViewModel entity)
    {
        // Search match can ALWAYS appear even if it does not match toggles
        if (_searchEntityId is not null)
        {
            // If the value matches exactly, return true
            if (entity.Id == _searchEntityId)
            {
                return true;
            }

            // If the value is a substring of the search value, return true
            var entityIdHex = entity.Id.ToString("X");
            if (entityIdHex.Contains(_searchEntityId.Value.ToString("X"), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        var hasSearchTerm = !string.IsNullOrWhiteSpace(SearchText);

        switch (hasSearchTerm)
        {
            case true when entity.Name?.Contains(SearchText, StringComparison.CurrentCulture) is true:
                return true;
            case true:
                return false;
        }

        if (entity.Flags.HasFlag(EntityFlags.Player) && !IncludePlayers)
        {
            return false;
        }

        if (entity.Flags.HasFlag(EntityFlags.Mundane) && !IncludeNpcs)
        {
            return false;
        }

        if (entity.Flags.HasFlag(EntityFlags.Monster) && !IncludeMonsters)
        {
            return false;
        }

        if (entity.Flags.HasFlag(EntityFlags.Item) && !IncludeItems)
        {
            return false;
        }

        if (entity.Flags.HasFlag(EntityFlags.Reactor) && !IncludeReactors)
        {
            return false;
        }

        return true;
    }

    private void OnEntityAdded(GameEntity entity)
    {
        var sortIndex = Interlocked.Increment(ref _indexCounter);
        var vm = new EntityViewModel(entity)
        {
            SortIndex = sortIndex
        };

        _allEntities.Add(vm);
    }

    private void OnEntityUpdated(GameEntity entity)
    {
        var vm = _allEntities.FirstOrDefault(vm => vm.Id == entity.Id);
        if (vm is null)
        {
            OnEntityAdded(entity);
            return;
        }

        var nameChanged = !string.Equals(vm.Name, entity.Name, StringComparison.OrdinalIgnoreCase);
        vm.Entity = entity;

        if (!nameChanged)
        {
            return;
        }

        if (!MatchesFilter(vm))
        {
            FilteredEntities.Remove(vm);
        }
        else if (MatchesFilter(vm) && !FilteredEntities.Contains(vm))
        {
            FilteredEntities.Add(vm);
        }
    }

    private void OnEntityRemoved(GameEntity entity)
    {
        var existingEntity = _allEntities.FirstOrDefault(vm => vm.Id == entity.Id);
        if (existingEntity is null)
        {
            return;
        }

        _allEntities.Remove(existingEntity);
    }

    partial void OnSearchTextChanged(string? oldValue, string newValue)
    {
        _searchRefreshDebouncer.Execute(() =>
        {
            if (uint.TryParse(newValue.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id))
            {
                _searchEntityId = id;
            }
            else
            {
                _searchEntityId = null;
            }

            FilteredEntities.Refresh();
        });
    }

    partial void OnIncludePlayersChanged(bool oldValue, bool newValue) =>
        _searchRefreshDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeNpcsChanged(bool oldValue, bool newValue) =>
        _searchRefreshDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeMonstersChanged(bool oldValue, bool newValue) =>
        _searchRefreshDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeItemsChanged(bool oldValue, bool newValue) =>
        _searchRefreshDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeReactorsChanged(bool oldValue, bool newValue) =>
        _searchRefreshDebouncer.Execute(() => FilteredEntities.Refresh());

}