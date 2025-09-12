using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerAddSkillMessage : INetworkSerializable
{
    public byte Slot { get; set; }
    public ushort Icon { get; set; }
    public string Name { get; set; } = string.Empty;

    public void Deserialize(INetworkPacketReader reader)
    {
        Slot = reader.ReadByte();
        Icon = reader.ReadUInt16();
        Name = reader.ReadString8();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}