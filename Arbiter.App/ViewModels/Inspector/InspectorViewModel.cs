using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
    private readonly InspectorViewModelFactory _factory;
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

    public InspectorViewModel(InspectorViewModelFactory factory)
    {
        _factory = factory;
    }

    private void OnPacketSelected(NetworkPacket? packet)
    {
        InspectedPacket = packet is not null ? _factory.Create(packet) : null;
    }
}