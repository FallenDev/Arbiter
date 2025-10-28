using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using Avalonia.Threading;

namespace Arbiter.App.Collections;

public class ConcurrentObservableCollection<T> : ObservableCollection<T>
{
    protected readonly Lock Lock = new();

    private bool _isDeferred;
    
    public Dispatcher Dispatcher { get; }

    public ConcurrentObservableCollection(Dispatcher? dispatcher = null)
    {
        Dispatcher = dispatcher ?? Dispatcher.UIThread;
    }

    public void DeferUpdates(Action action)
    {
        Lock.Enter();
        try
        {
            _isDeferred = true;
            WithinLock(action);
        }
        finally
        {
            _isDeferred = false;
            Lock.Exit();
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public void WithinLock(Action action)
    {
        Lock.Enter();
        try
        {
            action();
        }
        finally
        {
            Lock.Exit();
        }
    }

    protected override void InsertItem(int index, T item)
    {
        using var _ = Lock.EnterScope();
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
        using var _ = Lock.EnterScope();
        base.SetItem(index, item);
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        using var _ = Lock.EnterScope();
        base.MoveItem(oldIndex, newIndex);
    }

    protected override void RemoveItem(int index)
    {
        using var _ = Lock.EnterScope();
        base.RemoveItem(index);
    }
    
    protected override void ClearItems()
    {
        using var _ = Lock.EnterScope();
        base.ClearItems();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_isDeferred)
        {
            return;
        }
        
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Post(RaiseCollectionChanged);
            return;
        }
        
        RaiseCollectionChanged();
        return;

        void RaiseCollectionChanged()
        {
            using var _ = Lock.EnterScope();
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