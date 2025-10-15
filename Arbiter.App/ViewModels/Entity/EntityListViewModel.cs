using System;
using System.Linq;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.Threading;
using Arbiter.Net.Filters;
using Arbiter.Net.Proxy;
using Arbiter.Net.Server;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entity;

public partial class EntityListViewModel : ViewModelBase
{
    private readonly Debouncer _searchRefreshDebouncer = new(TimeSpan.FromMilliseconds(50), Dispatcher.UIThread);
    private readonly IEntityStore _entityStore;
    private readonly ConcurrentObservableCollection<EntityViewModel> _allEntities = [];

    [ObservableProperty] private string _searchText = string.Empty;

    public FilteredObservableCollection<EntityViewModel> FilteredEntities { get; }

    public EntityListViewModel(ProxyServer proxyServer, IEntityStore entityStore)
    {
        _entityStore = entityStore;
        _entityStore.EntityAdded += OnEntityAdded;
        _entityStore.EntityUpdated += OnEntityUpdated;
        _entityStore.EntityRemoved += OnEntityRemoved;

        FilteredEntities = new FilteredObservableCollection<EntityViewModel>(_allEntities, MatchesFilter);

        
    }

    partial void OnSearchTextChanged(string? oldValue, string newValue)
    {
        _searchRefreshDebouncer.Execute(() => { FilteredEntities.Refresh(); });
    }

    private bool MatchesFilter(EntityViewModel entity) => true;

    private void OnEntityAdded(GameEntity entity)
    {

    }

    private void OnEntityUpdated(GameEntity entity)
    {

    }

    private void OnEntityRemoved(GameEntity entity)
    {
        var existingEntity = _allEntities.FirstOrDefault(vm => vm.Id == entity.Id);
        if (existingEntity is null)
        {
            return;
        }

        _allEntities.Remove(existingEntity);
        FilteredEntities.Remove(existingEntity);
    }
}