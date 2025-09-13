using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.WorldMapClick)]
public class ClientWorldMapClickMessage : ClientMessage
{
    public ushort MapId { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort Checksum { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Checksum = reader.ReadUInt16();
        MapId = reader.ReadUInt16();
        X = reader.ReadUInt16();
        Y = reader.ReadUInt16();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}