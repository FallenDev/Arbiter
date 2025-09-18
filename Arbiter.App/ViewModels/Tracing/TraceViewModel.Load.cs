using System.IO;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    public async Task LoadFromFileAsync(string inputPath, bool append = false)
    {
        var traceFile = await _traceService.LoadTraceFileAsync(inputPath);
        var packets = traceFile.Packets;

        StopTracing();

        if (!append)
        {
            _allPackets.Clear();
        }

        foreach (var packet in packets)
        {
            var vm = TracePacketViewModel.FromTracePacket(packet, _packetDisplayMode);
            AddPacketToTrace(vm);
        }
        
        RefreshSearchResults();
    }
    
    [RelayCommand]
    private async Task LoadTrace()
    {
        var append = _keyboardService.IsModifierPressed(KeyModifiers.Shift);

        var tracesDirectory = await _storageProvider.TryGetFolderFromPathAsync(TracesDirectory);
        var result = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Trace File",
            FileTypeFilter = [JsonFileType],
            SuggestedFileName = "trace.json",
            SuggestedStartLocation = tracesDirectory,
            AllowMultiple = false
        });

        if (result.Count == 0)
        {
            return;
        }

        if (IsRunning && !IsEmpty)
        {
            var confirm = await _dialogService.ShowMessageBoxAsync(new MessageBoxDetails
            {
                Title = "Confirm Load Trace",
                Message = "You have an active trace running.\nAre you sure you want to load?",
                Description = append
                    ? "This will stop your current trace."
                    : "This will stop and replace your current trace.",
                Style = MessageBoxStyle.YesNo
            });

            if (confirm is not true)
            {
                return;
            }
        }

        var inputPath = result[0].Path.AbsolutePath;
        var filename = Path.GetFileName(inputPath);

        await LoadFromFileAsync(inputPath, append);
        _logger.LogInformation("Trace loaded from {Filename}", filename);
    }
}