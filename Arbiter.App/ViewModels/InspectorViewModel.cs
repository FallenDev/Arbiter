using Arbiter.App.Models.Packets;
using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class InspectorViewModel : ViewModelBase
{
    private readonly PacketMessageFactory _packetMessageFactory = new();
    
    private NetworkPacket? _selectedPacket;

    [ObservableProperty] private InspectorPacketViewModel? _inspectedPacket;
    
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
    
    public InspectorViewModel()
    {
        _packetMessageFactory.RegisterFromAssembly();
    }

    private void OnPacketSelected(NetworkPacket? packet)
    {
        if (packet is null || !_packetMessageFactory.CanParse(packet))
        {
            InspectedPacket = null;
            return;
        }

        if (!_packetMessageFactory.TryParsePacket(packet, out _, out _))
        {
            InspectedPacket = null;
            return;
        }

        InspectedPacket = new InspectorPacketViewModel();
    }
}