using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;

namespace Arbiter.App.Models.Server;

public class ServerItemEntity : ServerEntityObject
{
    [InspectProperty] public ItemColor Color { get; set; }

    [InspectProperty(ShowHex = true)] public ushort Unknown { get; set; }
}