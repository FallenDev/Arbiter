using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.LightLevel)]
public class ServerLightLevelMessage : ServerMessage
{
    public byte Brightness { get; set; }
    public byte Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Brightness = reader.ReadByte();
        Unknown = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendByte(Brightness);
        builder.AppendByte(Unknown);
    }
}