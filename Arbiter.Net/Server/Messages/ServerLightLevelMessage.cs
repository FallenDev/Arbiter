using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerLightLevelMessage : INetworkSerializable
{
    public byte Brightness { get; set; }
    public byte Unknown { get; set; }

    public void Deserialize(INetworkPacketReader reader)
    {
        Brightness = reader.ReadByte();
        Unknown = reader.ReadByte();
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}