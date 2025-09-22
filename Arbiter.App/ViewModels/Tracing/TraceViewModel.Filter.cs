using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Arbiter.App.Collections;
using Arbiter.App.Models;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private readonly Dictionary<string, Regex> _nameFilterRegexes = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty] private bool _showFilterBar;

    public FilteredObservableCollection<TracePacketViewModel> FilteredPackets { get; }
    public TraceFilterViewModel FilterParameters { get; } = new();

    private void OnFilterParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Do not filter on this, wait for pattern list to change
        if (e.PropertyName == nameof(FilterParameters.NameFilter))
        {
            return;
        }

        FilteredPackets.Refresh();
    }

    private bool MatchesFilter(TracePacketViewModel vm)
    {
        if (vm.Direction == PacketDirection.Client)
        {
            var selectedCommands = FilterParameters.SelectedClientCommands;
            if (selectedCommands.All(cmd => cmd.Value != vm.Command))
            {
                return false;
            }
        }
        else if (vm.Direction == PacketDirection.Server)
        {
            var selectedCommands = FilterParameters.SelectedServerCommands;
            if (selectedCommands.All(cmd => cmd.Value != vm.Command))
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