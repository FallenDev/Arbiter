using System;
using System.Net;
using Arbiter.Net;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
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

    private void OnPacketSelected(NetworkPacket? packet)
    {
        InspectedPacket = null;
    }
    
    private static bool IsCustomType(Type type) => type.IsClass && type != typeof(string) && type != typeof(IPAddress);
}