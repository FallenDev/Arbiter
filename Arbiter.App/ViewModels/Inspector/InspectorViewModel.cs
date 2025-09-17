using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
    private readonly ILogger<InspectorViewModel> _logger;
    private readonly InspectorViewModelFactory _factory;
    private NetworkPacket? _selectedPacket;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private InspectorPacketViewModel? _inspectedPacket;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private InspectorExceptionViewModel? _inspectorException;
    
    public bool IsEmpty => InspectedPacket is not null && InspectedPacket.Sections.Count == 0 && !HasError;
    public bool HasError => InspectorException is not null;
    
    public NetworkPacket? SelectedPacket
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

    private void OnPacketSelected(NetworkPacket? packet)
    {
        if (packet is null)
        {
            InspectedPacket = null;
            InspectorException = null;
            return;
        }

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
}