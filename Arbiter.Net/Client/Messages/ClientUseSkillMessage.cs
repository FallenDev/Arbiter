﻿using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.UseSkill)]
public class ClientUseSkillMessage : ClientMessage
{
    public byte Slot { get; set; }

    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Slot = reader.ReadByte();
    }

    public override void Serialize(NetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}