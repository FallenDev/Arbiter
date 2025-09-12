using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRequestUserPortraitMessage : ServerMessage
{
    public ushort Unknown { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Unknown = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}