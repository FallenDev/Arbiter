using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerMapChangingMessage : INetworkSerializable
{
    public byte ChangeType { get; set; }
    public uint Unknown { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        ChangeType = reader.ReadByte();
        Unknown = reader.ReadUInt32();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}