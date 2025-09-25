using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arbiter.App.Models;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Tracing;

public partial class TraceViewModel
{
    private static readonly string AutosaveDirectory = AppHelper.GetRelativePath("autosave");
    
    public Task SaveAllToFileAsync(string outputPath) => SaveToFileAsync(_allPackets, outputPath);
    
    public async Task AutoSaveTraceAsync()
    {
        if (!Directory.Exists(AutosaveDirectory))
        {
            Directory.CreateDirectory(AutosaveDirectory);
        }

        var defaultFilename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}-autosave.json";
        var outputPath = System.IO.Path.Join(AutosaveDirectory, defaultFilename);

        await SaveAllToFileAsync(outputPath);
        _logger.LogInformation("Autosaved trace to {Filename}", outputPath);
    }
    
    private async Task SaveToFileAsync(IEnumerable<TracePacketViewModel> viewModels, string outputPath)
    {
        var snapshot = viewModels.ToList();
        var packets = snapshot.Select(vm => vm.ToTracePacket());

        var traceFile = new TraceFile { Packets = packets.ToList() };
        await _traceService.SaveTraceFileAsync(traceFile, outputPath);
    }

    [RelayCommand]
    private async Task SaveAll()
    {
        var outputPath = await ShowSaveDialog("Save Trace");
        if (outputPath is null)
        {
            return;
        }

        await SaveAllToFileAsync(outputPath);
        IsDirty = false;
        
        _logger.LogInformation("Trace saved to {Filename}", Path.GetFileName(outputPath));
    }
    
    private bool CanSaveSelected() => SelectedPackets.Count > 0;

    [RelayCommand(CanExecute = nameof(CanSaveSelected))]
    private async Task SaveSelected()
    {
        var filename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}-selected.json";
        var outputPath = await ShowSaveDialog("Save Selected Packets", filename);

        if (outputPath is null)
        {
            return;
        }

        await SaveToFileAsync(SelectedPackets, outputPath);
        _logger.LogInformation("Selected packets saved to {Filename}", Path.GetFileName(outputPath));
    }

    private async Task<string?> ShowSaveDialog(string title, string? filename = null)
    {
        if (!Directory.Exists(TracesDirectory))
        {
            Directory.CreateDirectory(TracesDirectory);
        }

        var defaultFilename = StartTime != DateTime.MinValue
            ? $"{StartTime:yyyy-MM-dd_HH-mm-ss}-trace.json"
            : "trace.json";

        var tracesDirectory = await _storageProvider.TryGetFolderFromPathAsync(TracesDirectory);
        var result = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = [JsonFileType],
            SuggestedFileName = filename ?? defaultFilename,
            SuggestedStartLocation = tracesDirectory
        });

        var outputPath = result?.Path.AbsolutePath;
        return outputPath;
    }
}