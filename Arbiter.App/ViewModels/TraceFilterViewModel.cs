using System.Collections.Generic;
using Arbiter.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

public partial class TraceFilterViewModel : ViewModelBase
{
    public IReadOnlyList<PacketDirection> AvailablePacketDirections =>
        [PacketDirection.Client, PacketDirection.Server, PacketDirection.Both];

    [ObservableProperty] private PacketDirection _packetDirection = PacketDirection.Both;
}