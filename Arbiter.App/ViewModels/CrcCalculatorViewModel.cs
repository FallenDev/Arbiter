using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arbiter.App.ViewModels.Inspector;
using Arbiter.Net.Security;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels;

public partial class CrcCalculatorViewModel : ViewModelBase
{
    private const string Crc16AlgorithmName = "CRC-16";
    private const string Crc32AlgorithmName = "CRC-32";

    private readonly ILogger<CrcCalculatorViewModel> _logger;
    private readonly IStorageProvider _storageProvider;
    
    private readonly Crc16Provider _crc16 = new();
    private readonly Crc32Provider _crc32 = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private byte[] _inputBytes = [];

    [ObservableProperty] private string? _selectedAlgorithm;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsTextInput), nameof(IsBinaryInput), nameof(IsFileInput))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string? _selectedInputType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string _inputFilePath = string.Empty;

    [ObservableProperty] private double _progressPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand), nameof(CancelCalculationCommand))]
    private bool _isCalculating;

    public List<string> AvailableAlgorithms { get; } = [Crc16AlgorithmName, Crc32AlgorithmName];
    public List<string> AvailableInputTypes { get; } = ["Text", "Binary", "File"];

    public bool IsTextInput => SelectedInputType == "Text";
    public bool IsBinaryInput => SelectedInputType == "Binary";
    public bool IsFileInput => SelectedInputType == "File";

    public InspectorValueViewModel ChecksumValue { get; } = new()
    {
        Name = "Checksum",
        ToolTip = "The calculated CRC checksum value.",
        Value = null,
        ShowHex = true
    };

    public CrcCalculatorViewModel(ILogger<CrcCalculatorViewModel> logger, IStorageProvider storageProvider)
    {
        _logger = logger;
        _storageProvider = storageProvider;

        SelectedAlgorithm = AvailableAlgorithms.FirstOrDefault();
        SelectedInputType = AvailableInputTypes.FirstOrDefault();
    }

    private bool CanCalculate() => !IsCalculating && (IsTextInput && !string.IsNullOrEmpty(InputText) ||
                                                      (IsBinaryInput && _inputBytes.Length > 0) ||
                                                      (IsFileInput && !string.IsNullOrWhiteSpace(InputFilePath)));

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private async Task Calculate()
    {
        ProgressPercentage = 0;
        IsCalculating = true;

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
                Dispatcher.UIThread.Post(() => ProgressPercentage = value, DispatcherPriority.Background));

            var result = SelectedAlgorithm switch
            {
                Crc16AlgorithmName => await CalculateCrc16Async(progress, _cancellationTokenSource.Token),
                Crc32AlgorithmName => await CalculateCrc32Async(progress, _cancellationTokenSource.Token),
                _ => throw new InvalidOperationException("Invalid CRC algorithm")
            };

            ChecksumValue.Value = result;
            ProgressPercentage = 100;
        }
        catch (OperationCanceledException)
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            ChecksumValue.Value = null;
            _logger.LogError(ex, "Error while calculating {Algorithm}: {Message}", SelectedAlgorithm, ex.Message);
        }
        finally
        {
            ProgressPercentage = 0;
            IsCalculating = false;
        }
    }
    
    private bool CanCancel() => IsCalculating;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task CancelCalculation()
    {
        // Cancel the existing operation
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();

            while (IsCalculating)
            {
                await Task.Delay(100);
            }

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        IsCalculating = false;
        ProgressPercentage = 0;
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        var existingFolder =
            await _storageProvider.TryGetFolderFromPathAsync(
                Path.GetDirectoryName(InputFilePath) ?? string.Empty);

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select File",
            FileTypeFilter =
            [
                FilePickerFileTypes.All,
            ],
            SuggestedStartLocation = existingFolder,
            AllowMultiple = false
        });

        var selectedFile = files.Count > 0 ? files[0] : null;

        if (selectedFile is not null)
        {
            InputFilePath = selectedFile.TryGetLocalPath() ?? string.Empty;
        }
    }

    #region CRC-16

    private async Task<ushort> CalculateCrc16Async(IProgress<double> progress, CancellationToken token = default)
    {
        if (IsTextInput)
        {
            var bytes = Encoding.UTF8.GetBytes(InputText);
            return CalculateCrc16(bytes, 0, progress, token);
        }

        if (IsBinaryInput)
        {
            return CalculateCrc16(_inputBytes, 0, progress, token);
        }

        await using var stream = File.OpenRead(InputFilePath);
        var crc = await CalculateCrc16StreamAsync(stream, progress, token);

        return crc;
    }

    private ushort CalculateCrc16(ReadOnlyMemory<byte> bytes, ushort initial, IProgress<double> progress,
        CancellationToken token = default)
    {
        progress.Report(0);

        var crc = initial;

        for (var i = 0; i < bytes.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            crc = _crc16.ComputeNext(crc, bytes.Span[i]);

            progress.Report((i + 1) * 100.0 / _inputBytes.Length);
        }

        progress.Report(100);
        return crc;
    }

    private async Task<ushort> CalculateCrc16StreamAsync(Stream stream, IProgress<double> progress,
        CancellationToken token = default)
    {
        var readBuffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            var length = stream.Length;
            var current = 0;

            ushort crc = 0;
            while (current < length)
            {
                token.ThrowIfCancellationRequested();

                var readCount = await stream.ReadAsync(readBuffer, token);
                crc = CalculateCrc16(readBuffer.AsMemory(0, readCount), crc, progress, token);

                current += readCount;
                progress.Report(current * 100.0 / length);
            }

            progress.Report(100);
            return crc;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    #endregion

    #region CRC-32

    private async Task<uint> CalculateCrc32Async(IProgress<double> progress, CancellationToken token = default)
    {
        if (IsTextInput)
        {
            var bytes = Encoding.UTF8.GetBytes(InputText);
            return CalculateCrc32(bytes, 0, uint.MaxValue, progress, token);
        }

        if (IsBinaryInput)
        {
            return CalculateCrc32(_inputBytes, 0, uint.MaxValue, progress, token);
        }

        await using var stream = File.OpenRead(InputFilePath);
        var crc = await CalculateCrc32StreamAsync(stream, progress, token);

        return crc;
    }

    private uint CalculateCrc32(ReadOnlyMemory<byte> bytes, uint initial, uint finalXor, IProgress<double> progress,
        CancellationToken token = default)
    {
        progress.Report(0);

        var crc = initial;

        for (var i = 0; i < bytes.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            crc = _crc32.ComputeNext(crc, bytes.Span[i]);

            progress.Report((i + 1) * 100.0 / _inputBytes.Length);
        }

        progress.Report(100);
        return crc ^ finalXor;
    }

    private async Task<uint> CalculateCrc32StreamAsync(Stream stream, IProgress<double> progress,
        CancellationToken token = default)
    {
        var readBuffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            var length = stream.Length;
            var current = 0;

            var crc = uint.MaxValue;
            while (current < length)
            {
                token.ThrowIfCancellationRequested();

                var readCount = await stream.ReadAsync(readBuffer, token);
                crc = CalculateCrc32(readBuffer.AsMemory(0, readCount), crc, 0, progress, token);

                current += readCount;
                progress.Report(current * 100.0 / length);
            }

            progress.Report(100);
            return crc ^ uint.MaxValue;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    #endregion
}