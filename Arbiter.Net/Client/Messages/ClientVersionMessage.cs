using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.Version)]
public class ClientVersionMessage : ClientMessage
{
    public ushort Version { get; set; }
    public ushort Checksum { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Version = reader.ReadUInt16();

        // Seems to be 0x4C4B on 7.41
        Checksum = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        base.Serialize(builder);
        
        builder.AppendUInt16(Version);
        builder.AppendUInt16(Checksum);
    }
}