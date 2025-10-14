using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.LightLevel)]
public class ServerLightLevelMessage : ServerMessage
{
    public byte TimeOfDay { get; set; }
    public byte Lighting { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        TimeOfDay = reader.ReadByte();
        Lighting = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(TimeOfDay);
        builder.AppendByte(Lighting);
    }
}