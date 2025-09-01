using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;

namespace Arbiter.App.Models.Server;

[InspectTypeName("MapDoor")]
public class ServerMapDoor
{
    [InspectProperty] public byte X { get; set; }

    [InspectProperty] public byte Y { get; set; }

    [InspectProperty] public DoorDirection Direction { get; set; }

    [InspectProperty] public DoorState State { get; set; }
}