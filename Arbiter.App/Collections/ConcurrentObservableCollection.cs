using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using Avalonia.Threading;

namespace Arbiter.App.Collections;

public class ConcurrentObservableCollection<T> : ObservableCollection<T>
{
    private readonly Lock _lock = new();

    public Dispatcher Dispatcher { get; }

    public ConcurrentObservableCollection(Dispatcher? dispatcher = null)
    {
        Dispatcher = dispatcher ?? dispatcher ?? Dispatcher.UIThread;
    }

    public ConcurrentObservableCollection(IEnumerable<T> collection, Dispatcher? dispatcher = null)
        : base(collection)
    {
        Dispatcher = dispatcher ?? dispatcher ?? Dispatcher.UIThread;
    }

    public ConcurrentObservableCollection(IList<T> list, Dispatcher? dispatcher = null)
        : base(list)
    {
        Dispatcher = dispatcher ?? dispatcher ?? Dispatcher.UIThread;
    }

    protected override void InsertItem(int index, T item)
    {
        using var _ = _lock.EnterScope();
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
        using var _ = _lock.EnterScope();
        base.SetItem(index, item);
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        using var _ = _lock.EnterScope();
        base.MoveItem(oldIndex, newIndex);
    }

    protected override void RemoveItem(int index)
    {
        using var _ = _lock.EnterScope();
        base.RemoveItem(index);
    }
    
    protected override void ClearItems()
    {
        using var _ = _lock.EnterScope();
        base.ClearItems();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Post(RaiseCollectionChanged);
            return;
        }

        RaiseCollectionChanged();
        return;

        void RaiseCollectionChanged()
        {
            using var _ = _lock.EnterScope();
            base.OnCollectionChanged(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Post(RaisePropertyChanged);
            return;
        }
        
        RaisePropertyChanged();
        return;

        void RaisePropertyChanged() => base.OnPropertyChanged(e);
    }
}