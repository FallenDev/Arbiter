﻿using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.DropItem)]
public class ClientDropItemMessage : ClientMessage
{
    public byte Slot { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public uint Quantity { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Slot = reader.ReadByte();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
        Quantity = reader.ReadUInt32();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(Slot);
        builder.AppendUInt16(X);
        builder.AppendUInt16(Y);
        builder.AppendUInt32(Quantity);
    }
}