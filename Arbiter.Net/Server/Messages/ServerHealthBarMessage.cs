using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.HealthBar)]
public class ServerHealthBarMessage : ServerMessage
{
    public uint EntityId { get; set; }
    public byte Percent { get; set; }
    public byte Sound { get; set; }
    public byte Unknown { get; set; }
    
    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        EntityId = reader.ReadUInt32();
        Unknown = reader.ReadByte();
        Percent = reader.ReadByte();
        Sound = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}