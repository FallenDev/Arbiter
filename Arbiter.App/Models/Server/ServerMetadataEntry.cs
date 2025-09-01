using Arbiter.App.Annotations;

namespace Arbiter.App.Models.Server;

[InspectTypeName("MetadataEntry")]
public class ServerMetadataEntry
{
    [InspectProperty] public string Name { get; set; } = string.Empty;
    [InspectProperty(ShowHex = true)] public uint Checksum { get; set; }
}