using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.GiveCreatureItem)]
public class ClientGiveCreatureItemMessage : ClientMessage
{
    public byte Slot { get; set; }
    public uint EntityId { get; set; }
    public byte Quantity { get; set; }

    public override void Deserialize(INetworkPacketReader reader)
    {
        base.Deserialize(reader);

        Slot = reader.ReadByte();
        EntityId = reader.ReadUInt32();
        Quantity = reader.ReadByte();
    }

    public override void Serialize(INetworkPacketBuilder builder)
    {
        throw new NotImplementedException();
    }
}