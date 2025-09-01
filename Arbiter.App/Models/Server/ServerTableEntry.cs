using System.Net;
using Arbiter.App.Annotations;

namespace Arbiter.App.Models.Server;

[InspectTypeName("ServerTableEntry")]
public class ServerTableEntry
{
    [InspectProperty] public byte Id { get; set; }

    [InspectProperty] public string Name { get; set; } = string.Empty;

    [InspectProperty] public required IPAddress Address { get; set; }

    [InspectProperty] public ushort Port { get; set; }
}