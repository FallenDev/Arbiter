using System;
using System.Globalization;
using Arbiter.App.Threading;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entities;

public partial class EntityManagerViewModel
{
    private readonly Debouncer _searchRefreshDebouncer = new(TimeSpan.FromMilliseconds(50), Dispatcher.UIThread);

    private uint? _searchEntityId;
    [ObservableProperty] private string _searchText = string.Empty;

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

            // Adjust opacity of all entities instead of filtering
            foreach (var entity in _allEntities)
            {
                entity.Opacity = IsSearchMatch(entity) ? 1 : 0.5;
            }
        });
    }

    private bool IsSearchMatch(EntityViewModel entity)
    {
        var hasText = !string.IsNullOrWhiteSpace(SearchText);
        if (!hasText)
        {
            return true;
        }

        if (_searchEntityId is null)
        {
            return entity.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        if (_searchEntityId == entity.Id)
        {
            return true;
        }

        var entityIdHex = entity.Id.ToString("X");
        var searchHex = _searchEntityId.Value.ToString("X");

        if (entityIdHex.Contains(searchHex, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return entity.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}