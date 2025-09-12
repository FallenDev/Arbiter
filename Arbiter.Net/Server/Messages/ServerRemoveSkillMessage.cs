using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerRemoveSkillMessage : INetworkSerializable
{
    public byte Slot { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}