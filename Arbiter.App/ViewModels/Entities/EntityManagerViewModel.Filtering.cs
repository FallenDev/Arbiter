using System;
using System.Collections.Generic;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entities;

public partial class EntityManagerViewModel
{
    private readonly Debouncer _filterDebouncer = new(TimeSpan.FromMilliseconds(100), Dispatcher.UIThread);
    
    [ObservableProperty] private EntityFilterMode _filterMode = EntityFilterMode.Nearby;

    [ObservableProperty] private bool _includePlayers = true;
    [ObservableProperty] private bool _includeNpcs = true;
    [ObservableProperty] private bool _includeMonsters;
    [ObservableProperty] private bool _includeItems;
    [ObservableProperty] private bool _includeReactors;

    public List<EntityFilterMode> AvailableFilterModes =>
        [EntityFilterMode.All, EntityFilterMode.Map, EntityFilterMode.Nearby];
    
    public FilteredObservableCollection<EntityViewModel> FilteredEntities { get; }

    partial void OnFilterModeChanged(EntityFilterMode oldValue, EntityFilterMode newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludePlayersChanged(bool oldValue, bool newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeNpcsChanged(bool oldValue, bool newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeMonstersChanged(bool oldValue, bool newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeItemsChanged(bool oldValue, bool newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    partial void OnIncludeReactorsChanged(bool oldValue, bool newValue) =>
        _filterDebouncer.Execute(() => FilteredEntities.Refresh());

    private bool MatchesFilter(EntityViewModel entity)
    {
        // Check if map or nearby x/y are enabled
        if (FilterMode is EntityFilterMode.Map or EntityFilterMode.Nearby)
        {
            // If no client or no player state, do not show
            if (SelectedClient is null || !_playerService.TryGetState(SelectedClient.Id, out var player))
            {
                return false;
            }

            // Check if the entity is within range
            var isWithinRange = FilterMode switch
            {
                EntityFilterMode.Map => player.MapId == entity.MapId,
                EntityFilterMode.Nearby => player.MapId == entity.MapId && player is { MapX: not null, MapY: not null }
                                                                        && Math.Abs(player.MapX.Value - entity.X) < 15
                                                                        && Math.Abs(player.MapY.Value - entity.Y) < 15,
                _ => true
            };

            return isWithinRange;
        }

        return (IncludePlayers || !entity.Flags.HasFlag(EntityFlags.Player)) &&
               (IncludeNpcs || !entity.Flags.HasFlag(EntityFlags.Mundane)) &&
               (IncludeMonsters || !entity.Flags.HasFlag(EntityFlags.Monster)) &&
               (IncludeItems || !entity.Flags.HasFlag(EntityFlags.Item)) &&
               (IncludeReactors || !entity.Flags.HasFlag(EntityFlags.Reactor));
    }
}