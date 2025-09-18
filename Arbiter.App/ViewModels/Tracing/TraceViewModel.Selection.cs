using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    public event Action<TracePacketViewModel?>? SelectedPacketChanged;

    public int SelectedIndex => SelectedPackets.Count > 0 ? FilteredPackets.IndexOf(SelectedPackets[0]) : -1;
    public int SelectionCount => SelectedPackets.Count;
    
    public TracePacketViewModel? SelectedPacket => SelectedPackets.Count > 0 ? SelectedPackets[0] : null;
    public ObservableCollection<TracePacketViewModel> SelectedPackets { get; } = [];
    
    private void OnSelectedPacketsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedPacketChanged?.Invoke(SelectedPacket);

        Dispatcher.UIThread.Post(() =>
        {
            OnPropertyChanged(nameof(SelectedIndex));
            OnPropertyChanged(nameof(SelectionCount));
            OnPropertyChanged(nameof(SelectedPacket));
            
            CopyToClipboardCommand.NotifyCanExecuteChanged();
            DeleteSelectedCommand.NotifyCanExecuteChanged();
        }, DispatcherPriority.Background);
    }
    
    private void SelectItemByIndex(int index)
    {
        SelectedPackets.Clear();
        SelectedPackets.Add(FilteredPackets[index]);
        ScrollToIndexRequested = index;
    }
}