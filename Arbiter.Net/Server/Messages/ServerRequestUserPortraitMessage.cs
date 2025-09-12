using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRequestUserPortraitMessage : INetworkSerializable
{
    public ushort Unknown { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Unknown = reader.ReadUInt16();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}