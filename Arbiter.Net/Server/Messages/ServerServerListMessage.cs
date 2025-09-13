using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Server.Messages;

[NetworkCommand(ServerCommand.ServerList)]
public class ServerServerListMessage : ServerMessage
{
    public uint Checksum { get; set; }
    public byte Seed { get; set; }
    public IReadOnlyList<byte> PrivateKey { get; set; } = [];

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);
        
        // Not sure what this byte is for
        reader.Skip(1);

        Checksum = reader.ReadUInt32();

        Seed = reader.ReadByte();
        var keyLength = reader.ReadByte();
        PrivateKey = reader.ReadBytes(keyLength);
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}