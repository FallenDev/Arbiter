using System;
using System.Net;
using Arbiter.App.Mappings;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
    private readonly InspectorViewModelFactory _factory = new();
    private readonly InspectorMappingRegistry _mappingRegistry;
    private NetworkPacket? _selectedPacket;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private InspectorPacketViewModel? _inspectedPacket;

    public bool IsEmpty => InspectedPacket is not null && InspectedPacket.Sections.Count == 0;
    
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

    public InspectorViewModel(InspectorMappingRegistry mappingRegistry)
    {
        _mappingRegistry = mappingRegistry;
    }

    private void OnPacketSelected(NetworkPacket? packet)
    {
        InspectedPacket = null;

        if (packet is null)
        {
            return;
        }
        
        var vm = new InspectorPacketViewModel
        {
            Command = packet.Command,
            Direction = packet is ClientPacket ? PacketDirection.Client : PacketDirection.Server,
        };

        InspectedPacket = vm;
    }
}