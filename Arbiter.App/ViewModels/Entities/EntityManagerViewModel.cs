using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Services;
using Arbiter.App.ViewModels.Client;
using Arbiter.Net.Proxy;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.App.ViewModels.Entities;

public partial class EntityManagerViewModel : ViewModelBase
{
    private readonly IEntityStore _entityStore;
    private readonly IPlayerService _playerService;
    private readonly ConcurrentObservableCollection<EntityViewModel> _allEntities = [];

    private long _indexCounter = 1;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(InteractCommand))]
    private ClientViewModel? _selectedClient;

    [ObservableProperty] private bool _isItemSelected;

    public ObservableCollection<EntityViewModel> SelectedEntities { get; } = [];

    public EntityManagerViewModel(ProxyServer proxyServer, IServiceProvider serviceProvider)
    {
        _entityStore = serviceProvider.GetRequiredService<IEntityStore>();
        _playerService = serviceProvider.GetRequiredService<IPlayerService>();

        _entityStore.EntityAdded += OnEntityAdded;
        _entityStore.EntityUpdated += OnEntityUpdated;
        _entityStore.EntityRemoved += OnEntityRemoved;

        FilteredEntities = new FilteredObservableCollection<EntityViewModel>(_allEntities, MatchesFilter);

        SelectedEntities.CollectionChanged += (_, _) =>
        {
            InteractCommand.NotifyCanExecuteChanged();
            CopyIdToClipboardCommand.NotifyCanExecuteChanged();
            CopyHexToClipboardCommand.NotifyCanExecuteChanged();
            CopySpriteToClipboardCommand.NotifyCanExecuteChanged();
            DeleteSelectedCommand.NotifyCanExecuteChanged();

            IsItemSelected = SelectedEntities.Count > 0 && SelectedEntities.Any(x => x.Flags.HasFlag(EntityFlags.Item));
        };

        var clientManager = serviceProvider.GetRequiredService<ClientManagerViewModel>();
        clientManager.ClientSelected += OnClientSelected;

        AddObservers(proxyServer);
    }

    partial void OnSelectedClientChanged(ClientViewModel? oldValue, ClientViewModel? newValue)
    {
        if (FilterMode == EntityFilterMode.All)
        {
            return;
        }

        RefreshFilterPreservingSelection();
    }

    private void OnClientSelected(ClientViewModel? client)
    {
        SelectedClient = client;
    }

    private void OnEntityAdded(GameEntity entity)
    {
        var sortIndex = Interlocked.Increment(ref _indexCounter);
        var vm = new EntityViewModel(entity)
        {
            SortIndex = sortIndex
        };

        // Initialize opacity based on current search
        vm.Opacity = IsSearchMatch(vm) ? 1 : 0.5;

        if (!FilteredEntities.Dispatcher.CheckAccess())
        {
            FilteredEntities.Dispatcher.Post(() => InsertSorted(vm));
        }
        else
        {
            InsertSorted(vm);
        }
    }

    private void OnEntityUpdated(GameEntity entity)
    {
        EntityViewModel? vm = null;
        _allEntities.WithinLock(() =>
        {
            vm =  _allEntities.FirstOrDefault(e => e.Id == entity.Id);
        });
        
        if (vm is null)
        {
            OnEntityAdded(entity);
            return;
        }

        var nameChanged = !string.Equals(vm.Name, entity.Name, StringComparison.OrdinalIgnoreCase);
        vm.Entity = entity;

        if (!nameChanged && FilterMode == EntityFilterMode.All)
        {
            return;
        }

        // Update opacity whenever entity changes, as search can be by ID or name
        vm.Opacity = IsSearchMatch(vm) ? 1 : 0.5;

        if (!FilteredEntities.Dispatcher.CheckAccess())
        {
            FilteredEntities.Dispatcher.Post(() => UpdateFiltered(vm));
        }
        else
        {
            UpdateFiltered(vm);
        }

        // If currently sorting by Name and the name changed, re-apply sorting to preserve order
        if (nameChanged && SortOrder == EntitySortOrder.Name)
        {
            ResortAll(GetComparer());
        }
    }

    private void OnEntityRemoved(GameEntity entity)
    {
        EntityViewModel? vm = null;
        _allEntities.WithinLock(() => { vm = _allEntities.FirstOrDefault(e => e.Id == entity.Id); });

        if (vm is null)
        {
            return;
        }

        _allEntities.Remove(vm);
    }

    private void UpdateFiltered(EntityViewModel vm)
    {
        if (!MatchesFilter(vm))
        {
            FilteredEntities.Remove(vm);
        }
        else if (!FilteredEntities.Contains(vm))
        {
            FilteredEntities.Add(vm);
        }
    }
}