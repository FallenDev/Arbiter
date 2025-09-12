using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

public class ClientAuthenticateMessage : ClientMessage
{
    public byte Seed { get; set; }
    public IReadOnlyList<byte> PrivateKey { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public uint ConnectionId { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);
        Name = reader.ReadString8();
        ConnectionId = reader.ReadUInt32();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}