using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerPlaySoundMessage : ServerMessage
{
    public byte Sound { get; set; }
    public byte? Track { get; set; }
    public ushort? Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Sound = reader.ReadByte();
        
        if (Sound == 0xFF)
        {
            Track = reader.ReadByte();
            Unknown = reader.ReadUInt16();
        }
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}