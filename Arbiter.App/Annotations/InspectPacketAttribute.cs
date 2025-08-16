using System;
using Arbiter.App.Models;
using Arbiter.Net.Client;
using Arbiter.Net.Server;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class InspectPacketAttribute(PacketDirection direction, byte command, string name) : Attribute
{
    public PacketDirection Direction { get; set; } = direction;
    public byte Command { get; set; } = command;
    public string Name { get; set; } = name;

    public InspectPacketAttribute(ClientCommand command, string? name = null)
        : this(PacketDirection.Client, (byte)command, name ?? command.ToString())
    {
    }

    public InspectPacketAttribute(ServerCommand command, string? name = null)
        : this(PacketDirection.Server, (byte)command, name ?? command.ToString())
    {
    }
}