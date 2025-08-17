using System.Reflection;
using Arbiter.App.Annotations;
using Arbiter.App.Models;
using Arbiter.App.Models.Packets;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
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

        if (!_packetMessageFactory.TryParsePacket(packet, out var message, out var exception))
        {
            InspectedPacket = null;
            return;
        }

        var fallbackName = packet switch
        {
            ClientPacket clientPacket => clientPacket.Command.ToString(),
            ServerPacket serverPacket => serverPacket.Command.ToString(),
            _ => "Unknown"
        };

        InspectedPacket = new InspectorPacketViewModel
        {
            DisplayName = GetPacketDisplayName(message) ?? fallbackName,
            Direction = packet is ClientPacket ? PacketDirection.Client : PacketDirection.Server,
            Command = packet.Command,
            Exception = exception
        };
    }

    private static string? GetPacketDisplayName(IPacketMessage message)
        => message.GetType().GetCustomAttribute<InspectPacketAttribute>()?.Name;
}