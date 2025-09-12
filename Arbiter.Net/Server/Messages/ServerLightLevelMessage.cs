using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

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
        throw new NotImplementedException();
    }
}