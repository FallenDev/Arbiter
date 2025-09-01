using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;

namespace Arbiter.App.Models.Server;

public class ServerEntityObject
{
    [InspectProperty(ShowHex = true)] public uint Id { get; set; }

    [InspectProperty(ShowHex = true)] public ushort Sprite { get; set; }

    [InspectProperty] public ushort X { get; set; }

    [InspectProperty] public ushort Y { get; set; }
}