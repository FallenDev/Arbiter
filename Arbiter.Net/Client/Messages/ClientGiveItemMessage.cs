using Arbiter.Net.Annotations;
using Arbiter.Net.Serialization;

namespace Arbiter.Net.Client.Messages;

[NetworkCommand(ClientCommand.GiveItem)]
public class ClientGiveItemMessage : ClientMessage
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
        base.Serialize(builder);
        
        builder.AppendByte(Slot);
        builder.AppendUInt32(EntityId);
        builder.AppendByte(Quantity);
    }
}