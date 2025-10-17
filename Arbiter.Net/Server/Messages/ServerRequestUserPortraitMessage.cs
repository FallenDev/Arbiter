﻿using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.RequestUserPortrait)]
public class ServerRequestUserPortraitMessage : ServerMessage
{
    public override void Deserialize(NetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // Nothing to read
    }

    public override void Serialize(ref NetworkPacketBuilder builder)
    {
        base.Serialize(ref builder);
        
        // Nothing to write
    }
}