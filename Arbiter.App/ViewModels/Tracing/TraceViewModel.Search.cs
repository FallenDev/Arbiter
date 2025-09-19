using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private readonly List<int> _searchResultIndexes = [];

    [ObservableProperty] private bool _showSearchBar;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSearchResultsText))]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    private int _selectedSearchIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedSearchResultsText))]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    private int _searchResultCount;
    
    public TraceSearchViewModel SearchParameters { get; } = new();

    public string? FormattedSearchResultsText =>
        SearchParameters.Command is not null
            ? SearchResultCount > 0 ? $"{Math.Max(1, SelectedSearchIndex)} of {SearchResultCount}" : "no matches"
            : null;

    public bool HasSearchResults => SearchResultCount > 0;

    private void OnSearchParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Do not filter on this, wait for actual command byte to change
        if (e.PropertyName == nameof(SearchParameters.CommandFilter))
        {

        }

        RefreshSearchResults();
    }

    private void RefreshSearchResults()
    {
        _searchResultIndexes.Clear();
        SearchResultCount = 0;

        for (var i = 0; i < FilteredPackets.Count; i++)
        {
            var packet = FilteredPackets[i];
            var isMatch = MatchesSearch(packet);
            if (isMatch)
            {
                AddSearchResultIndex(i);
            }

            packet.Opacity = isMatch ? 1 : 0.5;
        }

        SelectedSearchIndex = 0;
    }

    private void AddSearchResultIndex(int index)
    {
        _searchResultIndexes.Add(index);
        SearchResultCount = _searchResultIndexes.Count;
    }

    private bool MatchesSearch(TracePacketViewModel vm)
    {
        if (SearchParameters.Command is null)
        {
            return true;
        }

        return vm.Command == SearchParameters.Command;
    }

    [RelayCommand]
    private void GotoPreviousSearchResult()
    {
        if (_searchResultIndexes.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedIndex;
        var pos = _searchResultIndexes.FindLastIndex(x => x < currentIndex);
        if (pos == -1)
        {
            pos = _searchResultIndexes.Count - 1;
        }

        SelectedSearchIndex = pos + 1;
        var packetIndex = _searchResultIndexes[pos];
        SelectItemByIndex(packetIndex);
    }

    [RelayCommand]
    private void GotoNextSearchResult()
    {
        if (_searchResultIndexes.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedIndex;
        var pos = _searchResultIndexes.FindIndex(x => x > currentIndex);
        if (pos == -1)
        {
            pos = 0;
        }

        SelectedSearchIndex = pos + 1;
        var packetIndex = _searchResultIndexes[pos];
        SelectItemByIndex(packetIndex);
    }
}