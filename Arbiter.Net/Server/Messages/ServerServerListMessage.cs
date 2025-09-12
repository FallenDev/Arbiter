using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

public class ServerServerListMessage : INetworkSerializable
{
    public uint Checksum { get; set; }
    public byte Seed { get; set; }
    public IReadOnlyList<byte> PrivateKey { get; set; } = [];

    public void Deserialize(INetworkPacketReader reader)
    {
        // Not sure what this byte is for
        reader.Skip(1);

        Checksum = reader.ReadUInt32();

        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);
    }

    public void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}