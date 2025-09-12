using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerExitResponseMessage : INetworkSerializable
{
    public byte Result { get; set; }
    public ushort Unknown { get; set; }
    
    public void Deserialize(INetworkPacketReader reader)
    {
        Result = reader.ReadByte();
        Unknown = reader.ReadUInt16();
    }
    
    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}