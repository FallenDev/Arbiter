using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Arbiter.App.Extensions;
using Arbiter.App.ViewModels.Tracing;
using Arbiter.Json.Converters;
using Arbiter.Net;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = { new IPAddressJsonConverter() }
    };
    
    private readonly ILogger<InspectorViewModel> _logger;
    private readonly InspectorViewModelFactory _factory;
    private TracePacketViewModel? _selectedPacket;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyCanExecuteChangedFor(nameof(CopyToClipboardCommand))]
    private InspectorPacketViewModel? _inspectedPacket;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private InspectorExceptionViewModel? _inspectorException;
    
    public bool IsEmpty => InspectedPacket is not null && InspectedPacket.Sections.Count == 0 && !HasError;
    public bool HasError => InspectorException is not null;
    
    public TracePacketViewModel? SelectedPacket
    {
        get => _selectedPacket;
        set
        {
            if (SetProperty(ref _selectedPacket, value))
            {
                OnPacketSelected(value);
            }
        }
    }

    public InspectorViewModel(ILogger<InspectorViewModel> logger, InspectorViewModelFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    private void OnPacketSelected(TracePacketViewModel? viewModel)
    {
        if (viewModel is null)
        {
            InspectedPacket = null;
            InspectorException = null;
            return;
        }

        var packet = viewModel.DecryptedPacket;
        var (vm, exception) = _factory.Create(packet);
        InspectedPacket = vm;
        
        if (exception is not null)
        {
            _logger.LogError(exception, "Failed to generate inspector view");
            InspectorException = new InspectorExceptionViewModel { Exception = exception };
        }
        else
        {
            InspectorException = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyToClipboard()
    {
        var clipboard = Application.Current?.TryGetClipboard();
        if (clipboard is null || InspectedPacket?.Value is null)
        {
            return;
        }

        try
        {
            var value = InspectedPacket?.Value;
            var jsonString = JsonSerializer.Serialize(value, JsonOptions);
            await clipboard.SetTextAsync(jsonString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy JSON to clipboard");
        }
    }

    private bool CanCopyToClipboard() => !IsEmpty && InspectedPacket?.Value is not null;
}