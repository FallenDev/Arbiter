using System.Collections.ObjectModel;
using Arbiter.App.Models;

namespace Arbiter.App.ViewModels;

public partial class InspectorPacketViewModel : ViewModelBase
{
    public string DisplayName { get; set; } = string.Empty;
    public PacketDirection Direction { get; set; }
    public byte Command { get; set; }

    public ObservableCollection<InspectorSectionViewModel> Sections { get; } = [];

    public ObservableCollection<InspectorItemViewModel> Items { get; } = [];
}