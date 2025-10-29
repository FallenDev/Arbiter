using System;
using Arbiter.App.Models.Entities;
using Arbiter.Net;
using Arbiter.Net.Server;

namespace Arbiter.App.Models;

internal readonly struct SendEntry
{
    public static SendEntry Disconnect => new() { IsDisconnect = true };
        
    public bool IsDisconnect { get; init; }
    public bool IsWait => Wait.HasValue;
    public NetworkPacket? Packet { get; }
    public TimeSpan? Wait { get; }


    public byte? Command { get; }
    public byte[] DataTemplate { get; } = [];
    public EntityRef[] EntityReferences { get; } = [];
    public bool IsServerPacket { get; }
    
    public SendEntry(NetworkPacket packet)
    {
        Packet = packet;
        IsServerPacket = packet is ServerPacket;
    }

    public SendEntry(TimeSpan wait)
    {
        Wait = wait;
    }

    public SendEntry(bool isServerPacket, byte command, byte[] dataTemplate, EntityRef[]? entityReferences)
    {
        Packet = null;
        Wait = null;
        Command = command;
        DataTemplate = dataTemplate;
        IsServerPacket = isServerPacket;

        if (entityReferences is not null)
        {
            EntityReferences = entityReferences;
        }
    }
}