using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Threading;

namespace Arbiter.App.Collections;

public class FilteredObservableCollection<T> : ConcurrentObservableCollection<T>, IDisposable
{
    private bool _isDisposed;

    private readonly ObservableCollection<T> _sourceCollection;
    private Func<T, bool> _predicate;

    public Func<T, bool> Predicate
    {
        get => _predicate;
        set
        {
            _predicate = value;
            Refresh();
        }
    }

    public FilteredObservableCollection(ObservableCollection<T> sourceCollection, Func<T, bool> predicate,
        Dispatcher? dispatcher = null
    ) : base(dispatcher)
    {
        _sourceCollection = sourceCollection ?? throw new ArgumentNullException(nameof(sourceCollection));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        _sourceCollection.CollectionChanged += OnCollectionChanged;
        Refresh();
    }

    public void Refresh()
    {
        using var _ = Lock.EnterScope();
        Clear();

        foreach (var item in _sourceCollection.Where(Predicate))
        {
            Add(item);
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems is not null:
            {
                // Insert new items at the correct relative positions based on the source index
                var newIndex = e.NewStartingIndex;
                var i = 0;
                foreach (var item in e.NewItems.OfType<T>())
                {
                    if (Predicate(item))
                    {
                        var targetIndex = GetFilteredIndexForSourceIndex(newIndex + i);
                        Insert(targetIndex, item);
                    }
                    i++;
                }
                break;
            }

            case NotifyCollectionChangedAction.Move when e.OldItems is not null:
            {
                // Move items to match the source order when the source moves them
                var newIndex = e.NewStartingIndex;
                var i = 0;
                foreach (var item in e.OldItems.OfType<T>())
                {
                    if (Predicate(item))
                    {
                        var currentIndex = IndexOf(item);
                        if (currentIndex >= 0)
                        {
                            var targetIndex = GetFilteredIndexForSourceIndex(newIndex + i);
                            if (currentIndex != targetIndex)
                            {
                                Move(currentIndex, targetIndex);
                            }
                        }
                    }
                    i++;
                }
                break;
            }

            case NotifyCollectionChangedAction.Remove when e.OldItems is not null:
                foreach (var item in e.OldItems.OfType<T>())
                {
                    Remove(item);
                }

                break;

            case NotifyCollectionChangedAction.Replace when e.OldItems is not null && e.NewItems is not null:
                foreach (var item in e.OldItems.OfType<T>())
                {
                    Remove(item);
                }

                {
                    var newIndex = e.NewStartingIndex;
                    var i = 0;
                    foreach (var item in e.NewItems.OfType<T>())
                    {
                        if (Predicate(item))
                        {
                            var targetIndex = GetFilteredIndexForSourceIndex(newIndex + i);
                            Insert(targetIndex, item);
                        }
                        i++;
                    }
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                Refresh();
                break;
        }
    }

    private int GetFilteredIndexForSourceIndex(int sourceIndex)
    {
        // Count how many items in the source match the predicate up to (but not including) sourceIndex
        var count = 0;
        for (var i = 0; i < Math.Min(sourceIndex, _sourceCollection.Count); i++)
        {
            var srcItem = _sourceCollection[i];
            if (Predicate(srcItem))
            {
                count++;
            }
        }
        return count;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            _sourceCollection.CollectionChanged -= OnCollectionChanged;
        }

        _isDisposed = true;
    }
}