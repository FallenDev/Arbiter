using System;
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
    private InspectorExceptionViewModel? _exception;
    
    public bool IsEmpty => InspectedPacket is not null && InspectedPacket.Sections.Count == 0;
    public bool HasError => Exception is not null;
    
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
        try
        {
            InspectedPacket = packet is not null ? _factory.Create(packet) : null;
            Exception = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate inspector view model for packet");
            
            InspectedPacket = null;
            Exception = new InspectorExceptionViewModel
            {
                Exception = ex
            };
        }
    }
}