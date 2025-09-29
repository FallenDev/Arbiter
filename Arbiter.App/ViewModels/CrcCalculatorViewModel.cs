using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

    private const string ZlibCompressionName = "Zlib";
    private const string NoCompressionName = "None";

    private readonly ILogger<CrcCalculatorViewModel> _logger;
    private readonly IStorageProvider _storageProvider;

    private readonly Crc16Provider _crc16 = new();
    private readonly Crc32Provider _crc32 = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private byte[] _inputBytes = [];

    [ObservableProperty] private string _selectedAlgorithm = Crc16AlgorithmName;

    [ObservableProperty] private string _selectedCompression = NoCompressionName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTextInput), nameof(IsBinaryInput), nameof(IsFileInput))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string _selectedInputType = "File";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string _inputFilePath = string.Empty;

    [ObservableProperty] private double _progressPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand), nameof(CancelCalculationCommand))]
    private bool _isCalculating;

    public List<string> AvailableAlgorithms { get; } = [Crc16AlgorithmName, Crc32AlgorithmName];
    public List<string> AvailableCompression { get; } = [ZlibCompressionName, NoCompressionName];
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

            switch (SelectedAlgorithm)
            {
                case Crc16AlgorithmName:
                {
                    var crc16 = await CalculateCrc16Async(progress, _cancellationTokenSource.Token);
                    ChecksumValue.Value = crc16;
                    break;
                }
                case Crc32AlgorithmName:
                {
                    var crc32 = await CalculateCrc32Async(progress, _cancellationTokenSource.Token);
                    ChecksumValue.Value = crc32;
                    break;
                }
                default:
                    throw new NotSupportedException("Unsupported CRC algorithm");
            }
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

    private async Task<Stream> GetDataStreamAsync()
    {
        Stream stream;

        if (IsTextInput)
        {
            var bytes = Encoding.UTF8.GetBytes(InputText);
            stream = new MemoryStream(bytes, writable: false);
        }
        else if (IsBinaryInput)
        {
            stream = new MemoryStream(_inputBytes, writable: false);
        }
        else if (IsFileInput)
        {
            var file = await _storageProvider.TryGetFileFromPathAsync(InputFilePath);
            if (file is null)
            {
                throw new FileNotFoundException("File not found");
            }

            stream = await file.OpenReadAsync();
        }
        else
        {
            throw new NotSupportedException("Unsupported input type");
        }

        if (SelectedCompression == ZlibCompressionName)
        {
            stream = new ZLibStream(stream, CompressionMode.Decompress);
        }

        return stream;
    }

    #region CRC-16

    private async Task<ushort> CalculateCrc16Async(IProgress<double> progress, CancellationToken token = default)
    {
        await using var stream = await GetDataStreamAsync();
        var readBuffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            progress.Report(0);

            var length = TryGetStreamLength(stream);
            var current = 0;

            ushort crc = 0;
            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                var readCount = await stream.ReadAsync(readBuffer, token);

                if (readCount <= 0)
                {
                    break;
                }

                crc = _crc16.Compute(readBuffer.AsSpan(0, readCount), crc, 0);
                current += readCount;

                if (length.HasValue)
                {
                    progress.Report(current * 100.0 / length.Value);
                }
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
        await using var stream = await GetDataStreamAsync();
        var readBuffer = ArrayPool<byte>.Shared.Rent(4096);

        try
        {
            progress.Report(0);

            var length = TryGetStreamLength(stream);
            var current = 0;

            var crc = uint.MaxValue;
            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                var readCount = await stream.ReadAsync(readBuffer, token);

                if (readCount <= 0)
                {
                    break;
                }

                crc = _crc32.Compute(readBuffer.AsSpan(0, readCount), crc, 0);
                current += readCount;

                if (length.HasValue)
                {
                    progress.Report(current * 100.0 / length.Value);
                }
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

    private static long? TryGetStreamLength(Stream stream)
    {
        try
        {
            return stream.Length;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }
}