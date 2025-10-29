using Arbiter.App.Mappings;
using Arbiter.App.Models.Tracing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.App.ViewModels.Inspector;

// This is only used at design-time to make it easier to adjust XAML view layout.
public class DesignInspectorViewModel : InspectorViewModel
{
    public DesignInspectorViewModel()
        : base(new NullLogger<InspectorViewModel>(), new InspectorViewModelFactory(new InspectorMappingRegistry()))
    {
        InspectedPacket = new InspectorPacketViewModel
        {
            DisplayName = "TestMessage",
            Command = 0x99,
            Direction = PacketDirection.Server
        };

        InspectedPacket.Sections.Add(new InspectorSectionViewModel
        {
            Header = "First Section",
            Items =
            {
                new InspectorValueViewModel { Name = "Test Value", Value = "Test" }
            },
            IsExpanded = true,
        });
    }
}