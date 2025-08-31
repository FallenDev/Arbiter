using System.Net;

namespace Arbiter.App.Models;

public record ServerTableEntry(int Id, string Name, IPAddress Address, ushort Port);