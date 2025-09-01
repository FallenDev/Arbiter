using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;

namespace Arbiter.App.Models.Server;

[InspectTypeName("ItemEntity")]
public class ServerItemEntity : ServerEntityObject
{
    [InspectProperty] public DyeColor DyeColor { get; set; }

    [InspectProperty(ShowHex = true)] public ushort Unknown { get; set; }
}