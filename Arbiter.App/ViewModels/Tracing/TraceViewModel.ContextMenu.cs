using System;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Arbiter.App.Models;
using Avalonia;
using CommunityToolkit.Mvvm.Input;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private bool CanCopyToClipboard() => SelectedPackets.Count > 0;

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyToClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null || SelectedPackets.Count == 0)
        {
            return;
        }

        var packetStrings = SelectedPackets.Select(vm => vm.DisplayMode switch
        {
            PacketDisplayMode.Decrypted => $"{vm.Command:X2} {vm.FormattedDecrypted}",
            _ => vm.FormattedEncrypted,
        });

        var lines = string.Join(Environment.NewLine, packetStrings);
        await clipboard.SetTextAsync(lines);
    }
    
    private bool CanDeleteSelected() => SelectedPackets.Count > 0;

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private void DeleteSelected()
    {
        if (SelectedPackets.Count == 0)
        {
            return;
        }
        
        var selectedPackets = SelectedPackets.ToList();
        foreach (var packet in selectedPackets)
        {
            _allPackets.Remove(packet);
        }

        SelectedPackets.Clear();
        RefreshSearchResults();
    }
}