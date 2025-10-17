﻿using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.RemoveSpell)]
public class ServerRemoveSpellMessage : ServerMessage
{
    public byte Slot { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(Slot);
    }
}