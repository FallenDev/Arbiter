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
    public async Task SaveToFileAsync(string outputPath)
    {
        var snapshot = _allPackets.ToList();
        var packets = snapshot.Select(vm => vm.ToTracePacket());

        var traceFile = new TraceFile { Packets = packets.ToList() };
        await _traceService.SaveTraceFileAsync(traceFile, outputPath);
    }
    
    [RelayCommand]
    private async Task SaveTrace()
    {
        if (!Directory.Exists(TracesDirectory))
        {
            Directory.CreateDirectory(TracesDirectory);
        }

        var tracesDirectory = await _storageProvider.TryGetFolderFromPathAsync(TracesDirectory);
        var result = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Trace File",
            FileTypeChoices = [JsonFileType],
            SuggestedFileName = $"{StartTime:yyyy-MM-dd_HH-mm-ss}-trace.json",
            SuggestedStartLocation = tracesDirectory
        });

        if (result is null)
        {
            return;
        }

        var outputPath = result.Path.AbsolutePath;
        var filename = Path.GetFileName(outputPath);

        await SaveToFileAsync(outputPath);
        _logger.LogInformation("Trace saved to {Filename}", filename);
    }
}