using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.App.Threading;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private readonly Debouncer _filterRefreshDebouncer = new(TimeSpan.FromMilliseconds(50), Dispatcher.UIThread);
    private readonly Dictionary<string, Regex> _nameFilterRegexes = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty] private bool _showFilterBar;

    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    public TraceFilterViewModel FilterParameters { get; } = new();

    private void OnFilterParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Ignore immediate changes to the raw NameFilter text; we'll react when NameFilterPatterns changes
        if (e.PropertyName == nameof(FilterParameters.NameFilter))
        {
            return;
        }

        RequestFilterRefresh();
    }

    private void RequestFilterRefresh()
    {
        // This is debounced to avoid excessive refreshing when multiple changes are made rapidly
        _filterRefreshDebouncer.Execute(() =>
        {
            FilteredPackets.Refresh();
        });
    }

    private bool MatchesFilter(TracePacketViewModel vm)
    {
        if (vm.Direction == PacketDirection.Client)
        {
            var packet = vm.DecryptedPacket as ClientPacket;
            var clientCommands = FilterParameters.SelectedClientCommands;
            if (clientCommands.All(cmd => cmd != (byte)packet!.Command))
            {
                return false;
            }
        }
        else if (vm.Direction == PacketDirection.Server)
        {
            var packet = vm.DecryptedPacket as ServerPacket;
            var serverCommands = FilterParameters.SelectedServerCommands;
            if (serverCommands.All(cmd => cmd != (byte)packet!.Command))
            {
                return false;
            }
        }
        
        // Filter by client name matches
        if (FilterParameters.NameFilterPatterns.Count > 0)
        {
            var nameMatches = FilterParameters.NameFilterPatterns.Any(namePattern =>
            {
                var regex = GetRegexForFilter(namePattern);
                return vm.ClientName is not null && regex.IsMatch(vm.ClientName);
            });

            if (!nameMatches)
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void SelectAllCommands()
    {
        foreach (var command in FilterParameters.Commands)
        {
            command.IsSelected = true;
        }
    }

    private Regex GetRegexForFilter(string namePattern)
    {
        if (_nameFilterRegexes.TryGetValue(namePattern, out var regex))
        {
            return regex;
        }

        var escaped = Regex.Escape(namePattern);
        var regexPattern = escaped.Replace("\\*", ".*").Replace("\\?", ".");
        var newRegex = new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        _nameFilterRegexes.Add(namePattern, newRegex);
        return newRegex;
    }
}