using Arbiter.App.Annotations;
using Arbiter.App.Models.Game;

namespace Arbiter.App.Models.Server;

public class ServerCreatureEntity : ServerEntityObject
{
    [InspectProperty] public CreatureType CreatureType { get; set; }

    [InspectProperty] public string? Name { get; set; }

    [InspectProperty] public WorldDirection Direction { get; set; }

    [InspectProperty(ShowHex = true)] public uint Unknown { get; set; }
}