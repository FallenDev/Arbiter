using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Entities;

public partial class EntityManagerViewModel
{
    [ObservableProperty] private EntitySortOrder _sortOrder = EntitySortOrder.FirstSeen;

    public List<EntitySortOrder> AvailableSortOrders =>
        [EntitySortOrder.FirstSeen, EntitySortOrder.Name, EntitySortOrder.Id];
    
    partial void OnSortOrderChanged(EntitySortOrder oldValue, EntitySortOrder newValue)
    {
        ResortAll(GetComparer(newValue));
    }

    private IComparer<EntityViewModel> GetComparer(EntitySortOrder? order = null)
    {
        var sortOrder = order ?? SortOrder;
        return sortOrder switch
        {
            EntitySortOrder.FirstSeen => Comparer<EntityViewModel>.Create((a, b) =>
            {
                var c = a.SortIndex.CompareTo(b.SortIndex);
                if (c != 0) return c;
                c = a.Id.CompareTo(b.Id);
                return c;
            }),
            EntitySortOrder.Id => Comparer<EntityViewModel>.Create((a, b) =>
            {
                var c = a.Id.CompareTo(b.Id);
                if (c != 0) return c;
                // Stable tiebreakers
                c = a.SortIndex.CompareTo(b.SortIndex);
                return c;
            }),
            EntitySortOrder.Name => Comparer<EntityViewModel>.Create((a, b) =>
            {
                if (string.IsNullOrEmpty(a.Name) && string.IsNullOrEmpty(b.Name)) return 0;
                if (string.IsNullOrEmpty(a.Name)) return 1;
                if (string.IsNullOrEmpty(b.Name)) return -1;
                var c = string.Compare(a.Name, b.Name, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
                if (c != 0) return c;
                c = a.Id.CompareTo(b.Id);
                return c != 0 ? c : a.SortIndex.CompareTo(b.SortIndex);
            }),
            _ => Comparer<EntityViewModel>.Default
        };
    }

    private void InsertSorted(EntityViewModel vm)
    {
        var comparer = GetComparer();
        var index = FindInsertIndex(vm, comparer);
        _allEntities.Insert(index, vm);
    }
    
    private int FindInsertIndex(EntityViewModel vm, IComparer<EntityViewModel> comparer)
    {
        // Binary search to find insertion index in the already-sorted list
        var min = 0;
        var max = _allEntities.Count;
        while (min < max)
        {
            var mid = (min + max) / 2;
            var cmp = comparer.Compare(_allEntities[mid], vm);
            if (cmp <= 0)
            {
                min = mid + 1;
            }
            else
            {
                max = mid;
            }
        }

        return min;
    }

    private void ResortAll(IComparer<EntityViewModel> comparer)
    {
        // Perform minimal moves to match the desired order
        var desired = _allEntities.OrderBy(e => e, comparer).ToList();
        for (var i = 0; i < desired.Count; i++)
        {
            var target = desired[i];
            if (ReferenceEquals(_allEntities[i], target))
            {
                continue;
            }

            var currentIndex = _allEntities.IndexOf(target);
            if (currentIndex >= 0)
            {
                _allEntities.Move(currentIndex, i);
            }
        }
    }
}