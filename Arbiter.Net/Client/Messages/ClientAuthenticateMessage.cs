using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.Authenticate)]
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
        base.Serialize(builder);
        builder.AppendByte(Seed);
        var keyBytes = PrivateKey?.ToArray() ?? Array.Empty<byte>();
        builder.AppendByte((byte)keyBytes.Length);
        if (keyBytes.Length > 0)
        {
            builder.AppendBytes(keyBytes);
        }
        builder.AppendString8(Name);
        builder.AppendUInt32(ConnectionId);
    }
}